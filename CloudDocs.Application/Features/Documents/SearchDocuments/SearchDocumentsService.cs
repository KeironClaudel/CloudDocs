using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.SearchDocuments;

/// <summary>
/// Provides operations for search documents.
/// </summary>
public class SearchDocumentsService : ISearchDocumentsService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentAccessService _documentAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDocumentsService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="documentAccessService">The document access service.</param>
    public SearchDocumentsService(
        IDocumentRepository documentRepository,
        IDocumentAccessService documentAccessService)
    {
        _documentRepository = documentRepository;
        _documentAccessService = documentAccessService;
    }

    /// <summary>
    /// Searches.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of document response.</returns>
    public async Task<PagedResult<DocumentResponse>> SearchAsync(
        User currentUser,
        SearchDocumentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _documentRepository.SearchAsync(request, cancellationToken);

        var filteredItems = result.Items
            .Where(x => _documentAccessService.CanAccessDocument(currentUser, x))
            .ToList();

        return new PagedResult<DocumentResponse>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = filteredItems.Count,
            Items = filteredItems.Select(x => new DocumentResponse(
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
                x.DocumentTypeId,
                x.DocumentType.Name,
                x.ExpirationDate,
                x.ExpirationDatePendingDefinition,
                x.AccessLevelId,
                x.AccessLevel.Name,
                x.AccessLevel.Code,
                x.Department,
                x.IsActive,
                x.CreatedAt)).ToList()
        };
    }
}