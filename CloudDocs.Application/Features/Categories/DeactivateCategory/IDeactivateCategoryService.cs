namespace CloudDocs.Application.Features.Categories.DeactivateCategory;

/// <summary>
/// Defines the contract for deactivate category operations.
/// </summary>
public interface IDeactivateCategoryService
{
    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}