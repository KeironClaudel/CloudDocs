using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.GetUserById;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Users;

/// <summary>
/// Contains tests for get user by id service.
/// </summary>
public class GetUserByIdServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await new GetUserByIdService(_userRepositoryMock.Object).GetByIdAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var user = new User
        {
            Id = userId,
            FullName = "Keiron",
            Email = "keiron@test.com",
            DepartmentId = departmentId,
            Department = new Department { Id = departmentId, Name = "IT" },
            RoleId = roleId,
            Role = new Role { Id = roleId, Name = "Admin" },
            IsActive = true,
            CreatedAt = createdAt
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await new GetUserByIdService(_userRepositoryMock.Object).GetByIdAsync(userId);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("Keiron");
        result.DepartmentName.Should().Be("IT");
        result.RoleName.Should().Be("Admin");
    }
}
