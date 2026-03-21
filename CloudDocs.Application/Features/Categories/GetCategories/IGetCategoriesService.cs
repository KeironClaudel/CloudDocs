using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategories;

public interface IGetCategoriesService
{
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}