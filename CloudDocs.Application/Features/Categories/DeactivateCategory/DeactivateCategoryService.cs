using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
namespace CloudDocs.Application.Features.Categories.DeactivateCategory;

public class DeactivateCategoryService : IDeactivateCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCategoryService(ICategoryRepository categoryRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

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