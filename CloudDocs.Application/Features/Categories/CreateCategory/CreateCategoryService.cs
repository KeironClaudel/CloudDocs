using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Categories.Common;
using CloudDocs.Domain.Entities;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Exceptions;

namespace CloudDocs.Application.Features.Categories.CreateCategory;

public class CreateCategoryService : ICreateCategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryService(ICategoryRepository categoryRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _categoryRepository.NameExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new BadRequestException("Category name is already in use.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                        null,
                                        "Create",
                                        "Categories",
                                        "Category",
                                        category.Id.ToString(),
                                        $"Category created: {category.Name}",
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