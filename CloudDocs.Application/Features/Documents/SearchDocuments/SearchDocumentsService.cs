using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.SearchDocuments;

public class SearchDocumentsService : ISearchDocumentsService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentAccessService _documentAccessService;

    public SearchDocumentsService(
        IDocumentRepository documentRepository,
        IDocumentAccessService documentAccessService)
    {
        _documentRepository = documentRepository;
        _documentAccessService = documentAccessService;
    }

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
                x.DocumentType,
                x.ExpirationDate,
                x.ExpirationDatePendingDefinition,
                x.AccessLevel,
                x.Department,
                x.IsActive,
                x.CreatedAt)).ToList()
        };
    }
}