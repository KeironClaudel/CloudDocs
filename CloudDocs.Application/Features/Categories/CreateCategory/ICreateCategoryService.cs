using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.CreateCategory;

/// <summary>
/// Defines the contract for create category operations.
/// </summary>
public interface ICreateCategoryService
{
    /// <summary>
    /// Creates.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response.</returns>
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}