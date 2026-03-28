namespace CloudDocs.Application.Features.Categories.ReactivateCategory;

public interface IReactivateCategoryService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
