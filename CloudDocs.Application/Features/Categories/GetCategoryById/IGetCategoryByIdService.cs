using CloudDocs.Application.Features.Categories.Common;

namespace CloudDocs.Application.Features.Categories.GetCategoryById;

public interface IGetCategoryByIdService
{
    Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}