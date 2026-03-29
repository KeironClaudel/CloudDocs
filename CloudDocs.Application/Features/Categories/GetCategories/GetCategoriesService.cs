using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategories;

/// <summary>
/// Provides operations for get categories.
/// </summary>
public class GetCategoriesService : IGetCategoriesService
{
    private readonly ICategoryRepository _categoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCategoriesService"/> class.
    /// </summary>
    /// <param name="categoryRepository">The category repository.</param>
    public GetCategoriesService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response list.</returns>
    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories.Select(x => new CategoryResponse(
            x.Id,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}