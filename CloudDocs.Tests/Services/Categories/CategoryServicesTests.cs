using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Categories.CreateCategory;
using CloudDocs.Application.Features.Categories.DeactivateCategory;
using CloudDocs.Application.Features.Categories.GetCategories;
using CloudDocs.Application.Features.Categories.GetCategoryById;
using CloudDocs.Application.Features.Categories.ReactivateCategory;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Categories;

/// <summary>
/// Contains tests for uncovered category services.
/// </summary>
public class CategoryServicesTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateAsync_ShouldThrowBadRequest_WhenNameAlreadyExists()
    {
        var request = new CreateCategoryRequest("Finance", null);
        _repositoryMock.Setup(x => x.NameExistsAsync(request.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await new CreateCategoryService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Category name is already in use.");
    }

    [Fact]
    public async Task GetAllAsync_ShouldMapCategories()
    {
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Finance", IsActive = true, CreatedAt = DateTime.UtcNow }
        });

        var result = await new GetCategoriesService(_repositoryMock.Object).GetAllAsync();

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Finance");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await new GetCategoryByIdService(_repositoryMock.Object).GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await new DeactivateCategoryService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .DeactivateAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateCategory_WhenCategoryExists()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Finance", IsActive = false, DeletedAt = DateTime.UtcNow };
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await new ReactivateCategoryService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .ReactivateAsync(id);

        result.Should().BeTrue();
        category.IsActive.Should().BeTrue();
        category.DeletedAt.Should().BeNull();
    }
}
