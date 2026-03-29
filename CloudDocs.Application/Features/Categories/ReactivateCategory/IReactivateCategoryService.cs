namespace CloudDocs.Application.Features.Categories.ReactivateCategory;

/// <summary>
/// Defines the contract for reactivate category operations.
/// </summary>
public interface IReactivateCategoryService
{
    /// <summary>
    /// Reactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
