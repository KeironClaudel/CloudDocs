using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentById;

public class GetDocumentByIdService : IGetDocumentByIdService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentAccessService _documentAccessService;

    public GetDocumentByIdService(
        IDocumentRepository documentRepository,
        IDocumentAccessService documentAccessService)
    {
        _documentRepository = documentRepository;
        _documentAccessService = documentAccessService;
    }

    public async Task<DocumentResponse?> GetByIdAsync(User currentUser, Guid id, CancellationToken cancellationToken = default)
    {
        var x = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (x is null)
            return null;

        if (!_documentAccessService.CanAccessDocument(currentUser, x))
            return null;

        return new DocumentResponse(
            x.Id,
            x.OriginalFileName,
            x.StoredFileName,
            x.FileExtension,
            x.ContentType,
            x.FileSize,
            x.CategoryId,
            x.Category.Name,
            x.UploadedByUserId,
            x.UploadedByUser.FullName,
            x.Month,
            x.Year,
            x.DocumentType,
            x.ExpirationDate,
            x.ExpirationDatePendingDefinition,
            x.AccessLevel,
            x.Department,
            x.IsActive,
            x.CreatedAt);
    }
}