using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Categories.Common;
using CloudDocs.Application.Features.Categories.UpdateCategory;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Categories;

/// <summary>
/// Contains tests for update category service.
/// </summary>
public class UpdateCategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private UpdateCategoryService CreateService()
    {
        return new UpdateCategoryService(
            _categoryRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that update category should return null when category does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest("New Category", "Description");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(categoryId, request);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that update category should throw exception when name already exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperation_WhenNameAlreadyExists()
    {
        var categoryId = Guid.NewGuid();
        var newName = "Invoices";
        var request = new UpdateCategoryRequest(newName, "Description");

        var category = new Category
        {
            Id = categoryId,
            Name = "Receipts",
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.NameExistsAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var act = async () => await service.UpdateAsync(categoryId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category name is already in use.");
    }

    /// <summary>
    /// Verifies that update category should update successfully when name is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully_WhenNameIsValid()
    {
        var categoryId = Guid.NewGuid();
        var newName = "Updated Category";
        var newDescription = "Updated Description";
        var request = new UpdateCategoryRequest(newName, newDescription);

        var category = new Category
        {
            Id = categoryId,
            Name = "Original Category",
            Description = "Original Description",
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.NameExistsAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var result = await service.UpdateAsync(categoryId, request);

        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be(newName);
        result.Description.Should().Be(newDescription);

        _categoryRepositoryMock.Verify(
            x => x.UpdateAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                null,
                "Update",
                "Categories",
                "Category",
                categoryId.ToString(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that update category should trim whitespace from names.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldTrimWhitespace_WhenUpdatingCategoryName()
    {
        var categoryId = Guid.NewGuid();
        var newNameWithSpace = "  Updated Category  ";
        var expectedName = "Updated Category";
        var request = new UpdateCategoryRequest(newNameWithSpace, "  Description  ");

        var category = new Category
        {
            Id = categoryId,
            Name = "Original",
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.NameExistsAsync(newNameWithSpace, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var result = await service.UpdateAsync(categoryId, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be(expectedName);
        result.Description.Should().Be("Description");
    }

    /// <summary>
    /// Verifies that update category should allow same name as current.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldAllowSameName_WhenUpdatingCurrentName()
    {
        var categoryId = Guid.NewGuid();
        var sameName = "Finance Documents";
        var request = new UpdateCategoryRequest(sameName, null);

        var category = new Category
        {
            Id = categoryId,
            Name = sameName,
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.NameExistsAsync(sameName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.UpdateAsync(categoryId, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be(sameName);
    }
}
