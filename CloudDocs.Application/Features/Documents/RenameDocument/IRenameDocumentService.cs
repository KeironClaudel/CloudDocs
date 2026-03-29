namespace CloudDocs.Application.Features.Documents.RenameDocument;

/// <summary>
/// Defines the contract for rename document operations.
/// </summary>
public interface IRenameDocumentService
{
    /// <summary>
    /// Renames.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    Task<bool> RenameAsync(Guid id, RenameDocumentRequest request, CancellationToken cancellationToken = default);
}