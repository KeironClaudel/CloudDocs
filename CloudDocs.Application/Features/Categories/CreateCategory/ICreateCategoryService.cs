using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.CreateCategory;

public interface ICreateCategoryService
{
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}