using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentFile;

/// <summary>
/// Provides operations for get document file.
/// </summary>
public class GetDocumentFileService : IGetDocumentFileService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditService _auditService;
    private readonly IDocumentAccessService _documentAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentFileService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="documentAccessService">The document access service.</param>
    public GetDocumentFileService(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IAuditService auditService,
        IDocumentAccessService documentAccessService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _auditService = auditService;
        _documentAccessService = documentAccessService;
    }

    /// <summary>
    /// Gets the file.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="actorUserId">The actor user id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the (stream?stream,string file name,string content type) when available; otherwise, null.</returns>
    public async Task<(Stream? Stream, string FileName, string ContentType)?> GetFileAsync(
        User currentUser,
        Guid id,
        string action,
        Guid? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return null;

        if (!_documentAccessService.CanAccessDocument(currentUser, document))
            return null;

        var stream = await _fileStorageService.GetFileAsync(document.StoragePath, cancellationToken);
        if (stream is null)
            return null;

        await _auditService.LogAsync(
            actorUserId,
            action,
            "Documents",
            "Document",
            document.Id.ToString(),
            $"{action} document: {document.OriginalFileName}{document.FileExtension}",
            null,
            cancellationToken);

        return (stream, $"{document.OriginalFileName}{document.FileExtension}", document.ContentType);
    }
}