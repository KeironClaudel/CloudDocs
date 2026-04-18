using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.AccessLevels.DeactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevelById;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevels;
using CloudDocs.Application.Features.AccessLevels.ReactivateAccessLevel;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.AccessLevels;

/// <summary>
/// Contains tests for uncovered access level services.
/// </summary>
public class AccessLevelServicesTests
{
    private readonly Mock<IAccessLevelRepository> _repositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task GetAllAsync_ShouldMapEntities()
    {
        var createdAt = DateTime.UtcNow;
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AccessLevelEntity>
        {
            new() { Id = Guid.NewGuid(), Code = "INTERNAL_PUBLIC", Name = "Publico", IsActive = true, CreatedAt = createdAt }
        });

        var result = await new GetAccessLevelsService(_repositoryMock.Object).GetAllAsync();

        result.Should().ContainSingle();
        result[0].Code.Should().Be("INTERNAL_PUBLIC");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((AccessLevelEntity?)null);

        var result = await new GetAccessLevelByIdService(_repositoryMock.Object).GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateAsync_ShouldThrowBadRequest_WhenCodeIsProtected()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(
            new AccessLevelEntity { Id = id, Code = "ADMIN_ONLY", Name = "Admins", IsActive = true });

        var service = new DeactivateAccessLevelService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object);
        var act = async () => await service.DeactivateAsync(id);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Core access levels cannot be deactivated.");
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((AccessLevelEntity?)null);

        var result = await new ReactivateAccessLevelService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .ReactivateAsync(id);

        result.Should().BeFalse();
    }
}
