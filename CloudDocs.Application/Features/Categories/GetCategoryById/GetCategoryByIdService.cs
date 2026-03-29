using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategoryById;

/// <summary>
/// Provides operations for get category by id.
/// </summary>
public class GetCategoryByIdService : IGetCategoryByIdService
{
    private readonly ICategoryRepository _categoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCategoryByIdService"/> class.
    /// </summary>
    /// <param name="categoryRepository">The category repository.</param>
    public GetCategoryByIdService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response when available; otherwise, null.</returns>
    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return null;

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt);
    }
}