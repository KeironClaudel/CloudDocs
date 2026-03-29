using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategoryById;

/// <summary>
/// Defines the contract for get category by id operations.
/// </summary>
public interface IGetCategoryByIdService
{
    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response when available; otherwise, null.</returns>
    Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}