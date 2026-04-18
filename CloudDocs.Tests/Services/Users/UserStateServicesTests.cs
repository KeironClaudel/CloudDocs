using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Users.DeactivateUser;
using CloudDocs.Application.Features.Users.ReactivateUser;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Users;

/// <summary>
/// Contains tests for user state services without previous coverage.
/// </summary>
public class UserStateServicesTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task DeactivateAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await new DeactivateUserService(_userRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .DeactivateAsync(userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FullName = "User",
            IsActive = false,
            DeletedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await new ReactivateUserService(_userRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .ReactivateAsync(userId);

        result.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        user.DeletedAt.Should().BeNull();
        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
