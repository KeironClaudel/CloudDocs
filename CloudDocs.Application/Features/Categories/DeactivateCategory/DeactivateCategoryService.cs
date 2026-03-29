using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
namespace CloudDocs.Application.Features.Categories.DeactivateCategory;

/// <summary>
/// Provides operations for deactivate category.
/// </summary>
public class DeactivateCategoryService : IDeactivateCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeactivateCategoryService"/> class.
    /// </summary>
    /// <param name="categoryRepository">The category repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeactivateCategoryService(ICategoryRepository categoryRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return false;

        category.IsActive = false;
        category.DeletedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                        null,
                                        "Deactivate",
                                        "Categories",
                                        "Category",
                                        category.Id.ToString(),
                                        $"Category deactivated: {category.Name}",
                                        null,
                                        cancellationToken);

        return true;
    }
}