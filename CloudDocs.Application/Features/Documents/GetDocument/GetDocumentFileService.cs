using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentFile;

public class GetDocumentFileService : IGetDocumentFileService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditService _auditService;
    private readonly IDocumentAccessService _documentAccessService;

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