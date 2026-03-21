namespace CloudDocs.Application.Features.Categories.DeactivateCategory;

public interface IDeactivateCategoryService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}