using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentById;

/// <summary>
/// Provides operations for get document by id.
/// </summary>
public class GetDocumentByIdService : IGetDocumentByIdService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentAccessService _documentAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentByIdService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="documentAccessService">The document access service.</param>
    public GetDocumentByIdService(
        IDocumentRepository documentRepository,
        IDocumentAccessService documentAccessService)
    {
        _documentRepository = documentRepository;
        _documentAccessService = documentAccessService;
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document response when available; otherwise, null.</returns>
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