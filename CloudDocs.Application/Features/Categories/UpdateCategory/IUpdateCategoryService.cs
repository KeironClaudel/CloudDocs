using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.UpdateCategory;

public interface IUpdateCategoryService
{
    Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
}