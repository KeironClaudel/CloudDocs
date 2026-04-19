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

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDocumentsService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    public SearchDocumentsService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
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
        var result = await _documentRepository.SearchAsync(currentUser, request, cancellationToken);

        return new PagedResult<DocumentResponse>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new DocumentResponse(
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
                x.ClientId,
                x.Client.Name,
                x.DocumentDepartments
                    .Select(dd => new DocumentDepartmentResponse(
                        dd.DepartmentId,
                        dd.Department.Name))
                    .ToList(),
                x.IsActive,
                x.CreatedAt)).ToList()
        };
    }
}
