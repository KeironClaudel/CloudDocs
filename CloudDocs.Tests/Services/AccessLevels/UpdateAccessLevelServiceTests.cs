using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.AccessLevels.Common;
using CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.AccessLevels;

/// <summary>
/// Contains tests for update access level service.
/// </summary>
public class UpdateAccessLevelServiceTests
{
    private readonly Mock<IAccessLevelRepository> _accessLevelRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private UpdateAccessLevelService CreateService()
    {
        return new UpdateAccessLevelService(
            _accessLevelRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that update access level should return null when access level does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenAccessLevelDoesNotExist()
    {
        var levelId = Guid.NewGuid();
        var request = new UpdateAccessLevelRequest("New Name", "New Description");

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessLevelEntity?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(levelId, request);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that update access level should throw exception when name already exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequest_WhenNameAlreadyExists()
    {
        var levelId = Guid.NewGuid();
        var newName = "Another Access Level";
        var request = new UpdateAccessLevelRequest(newName, "Description");

        var accessLevel = new AccessLevelEntity
        {
            Id = levelId,
            Code = "EXISTING",
            Name = "Existing Access Level",
            IsActive = true
        };

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessLevel);

        _accessLevelRepositoryMock
            .Setup(x => x.NameExistsAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var act = async () => await service.UpdateAsync(levelId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Access level name is already in use.");
    }

    /// <summary>
    /// Verifies that update access level should update successfully when name is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully_WhenNameIsValid()
    {
        var levelId = Guid.NewGuid();
        var newName = "Updated Access Level";
        var newDescription = "Updated Description";
        var request = new UpdateAccessLevelRequest(newName, newDescription);

        var accessLevel = new AccessLevelEntity
        {
            Id = levelId,
            Code = "EXISTING",
            Name = "Original Name",
            Description = "Original Description",
            IsActive = true,
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessLevel);

        _accessLevelRepositoryMock
            .Setup(x => x.NameExistsAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var result = await service.UpdateAsync(levelId, request);

        result.Should().NotBeNull();
        result!.Id.Should().Be(levelId);
        result.Name.Should().Be(newName);
        result.Description.Should().Be(newDescription);

        _accessLevelRepositoryMock.Verify(
            x => x.UpdateAsync(accessLevel, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that update access level should allow same name as current.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldAllowSameName_WhenUpdatingCurrentName()
    {
        var levelId = Guid.NewGuid();
        var sameName = "Same Access Level";
        var request = new UpdateAccessLevelRequest(sameName, null);

        var accessLevel = new AccessLevelEntity
        {
            Id = levelId,
            Code = "SAME",
            Name = sameName,
            IsActive = true
        };

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(levelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessLevel);

        _accessLevelRepositoryMock
            .Setup(x => x.NameExistsAsync(sameName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.UpdateAsync(levelId, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be(sameName);
    }
}
