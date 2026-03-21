using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategories;

public class GetCategoriesService : IGetCategoriesService
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

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