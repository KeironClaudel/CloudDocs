using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentFile;

/// <summary>
/// Provides operations for get document file.
/// </summary>
public class GetDocumentFileService : IGetDocumentFileService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogQueue _auditLogQueue;
    private readonly IDocumentAccessService _documentAccessService;
    private readonly IDocumentVersionRepository _documentVersionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentFileService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    /// <param name="auditLogQueue">The audit log queue.</param>
    /// <param name="documentAccessService">The document access service.</param>
    /// <param name="documentVersionRepository">The document version repository.</param>
    public GetDocumentFileService(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IAuditLogQueue auditLogQueue,
        IDocumentAccessService documentAccessService,
        IDocumentVersionRepository documentVersionRepository)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _auditLogQueue = auditLogQueue;
        _documentAccessService = documentAccessService;
        _documentVersionRepository = documentVersionRepository;
    }

    /// <summary>
    /// Gets the file.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="actorUserId">The actor user id identifier.</param>
    /// <param name="versionId">The document version identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the (stream?stream,string file name,string content type) when available; otherwise, null.</returns>
    public async Task<(Stream? Stream, string FileName, string ContentType)?> GetFileAsync(
        User currentUser,
        Guid documentId,
        string action,
        Guid? actorUserId = null,
        Guid? versionId = null,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            return null;

        if (!_documentAccessService.CanAccessDocument(currentUser, document))
            return null;

        string storagePath;
        string fileName;
        string contentType;

        if (versionId.HasValue)
        {
            var version = await _documentVersionRepository.GetByIdAsync(versionId.Value, cancellationToken);
            if (version is null)
                return null;

            if (version.DocumentId != documentId)
                return null;

            storagePath = version.StoragePath;
            fileName = $"{document.OriginalFileName}_v{version.VersionNumber}{document.FileExtension}";
            contentType = document.ContentType;

            await _auditLogQueue.QueueAsync(
                new AuditLogRequest(
                    actorUserId,
                    action,
                    "Documents",
                    "DocumentVersion",
                    version.Id.ToString(),
                    $"{action} document version {version.VersionNumber} for: {document.OriginalFileName}{document.FileExtension}"),
                cancellationToken);
        }
        else
        {
            storagePath = document.StoragePath;
            fileName = $"{document.OriginalFileName}{document.FileExtension}";
            contentType = document.ContentType;

            await _auditLogQueue.QueueAsync(
                new AuditLogRequest(
                    actorUserId,
                    action,
                    "Documents",
                    "Document",
                    document.Id.ToString(),
                    $"{action} document: {document.OriginalFileName}{document.FileExtension}"),
                cancellationToken);
        }

        var stream = await _fileStorageService.GetFileAsync(storagePath, cancellationToken);
        if (stream is null)
            return null;

        return (stream, fileName, contentType);
    }
}
