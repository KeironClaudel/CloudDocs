using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Categories.Common;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Categories.UpdateCategory;

/// <summary>
/// Provides operations for update category.
/// </summary>
public class UpdateCategoryService : IUpdateCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCategoryService"/> class.
    /// </summary>
    /// <param name="categoryRepository">The category repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateCategoryService(ICategoryRepository categoryRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category response when available; otherwise, null.</returns>
    public async Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return null;

        var normalizedRequestedName = request.Name.Trim().ToLower();
        var nameExists = await _categoryRepository.NameExistsAsync(request.Name, cancellationToken);

        if (nameExists && category.Name.Trim().ToLower() != normalizedRequestedName)
            throw new InvalidOperationException("Category name is already in use.");

        category.Name = request.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                    null,
                                    "Update",
                                    "Categories",
                                    "Category",
                                    category.Id.ToString(),
                                    $"Category updated: {category.Name}",
                                    null,
                                    cancellationToken);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt);
    }
}