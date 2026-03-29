using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentById;

/// <summary>
/// Defines the contract for get document by id operations.
/// </summary>
public interface IGetDocumentByIdService
{
    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document response when available; otherwise, null.</returns>
    Task<DocumentResponse?> GetByIdAsync(User currentUser, Guid id, CancellationToken cancellationToken = default);
}