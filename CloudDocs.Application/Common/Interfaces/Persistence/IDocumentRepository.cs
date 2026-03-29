using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for document persistence operations.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document when available; otherwise, null.</returns>
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    /// <summary>
    /// Searches.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of document.</returns>
    Task<PagedResult<Document>> SearchAsync(SearchDocumentsRequest request, CancellationToken cancellationToken = default);
}