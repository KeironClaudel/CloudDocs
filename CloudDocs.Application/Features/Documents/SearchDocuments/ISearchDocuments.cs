using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.SearchDocuments;

/// <summary>
/// Defines the contract for search documents operations.
/// </summary>
public interface ISearchDocumentsService
{
    /// <summary>
    /// Searches.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of document response.</returns>
    Task<PagedResult<DocumentResponse>> SearchAsync(
        User currentUser,
        SearchDocumentsRequest request,
        CancellationToken cancellationToken = default);
}