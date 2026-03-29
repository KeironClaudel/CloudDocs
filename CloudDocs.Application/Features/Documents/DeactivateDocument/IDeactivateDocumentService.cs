namespace CloudDocs.Application.Features.Documents.DeactivateDocument;

/// <summary>
/// Defines the contract for deactivate document operations.
/// </summary>
public interface IDeactivateDocumentService
{
    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}