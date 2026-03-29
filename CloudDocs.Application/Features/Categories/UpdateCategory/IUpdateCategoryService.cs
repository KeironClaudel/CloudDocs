using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.UpdateCategory;

/// <summary>
/// Defines the contract for update category operations.
/// </summary>
public interface IUpdateCategoryService
{
    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response when available; otherwise, null.</returns>
    Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
}