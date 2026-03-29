using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategories;

/// <summary>
/// Defines the contract for get categories operations.
/// </summary>
public interface IGetCategoriesService
{
    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response list.</returns>
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}