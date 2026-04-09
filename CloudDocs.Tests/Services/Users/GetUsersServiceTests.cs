using CloudDocs.Application.Features.Users.Common;
using CloudDocs.Application.Features.Users.GetUsers;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Users;

/// <summary>
/// Contains tests for get users service.
/// </summary>
public class GetUsersServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    private GetUsersService CreateService()
    {
        return new GetUsersService(_userRepositoryMock.Object);
    }

    /// <summary>
    /// Verifies that get all users should return empty list when no users exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var service = CreateService();
        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that get all users should return all users with their details.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers_WhenUsersExist()
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "User One",
                Email = "user1@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Role = new Role { Name = "Admin" },
                Department = new Department { Name = "IT" }
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "User Two",
                Email = "user2@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Role = new Role { Name = "User" },
                Department = null
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "User Three",
                Email = "user3@test.com",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                Role = new Role { Name = "User" },
                Department = new Department { Name = "Sales" }
            }
        };

        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var service = CreateService();
        var result = await service.GetAllAsync();

        result.Should().HaveCount(3);

        result[0].FullName.Should().Be("User One");
        result[0].Email.Should().Be("user1@test.com");
        result[0].Role.Should().Be("Admin");
        result[0].Department.Should().Be("IT");
        result[0].IsActive.Should().BeTrue();

        result[1].FullName.Should().Be("User Two");
        result[1].Email.Should().Be("user2@test.com");
        result[1].Role.Should().Be("User");
        result[1].Department.Should().BeNull();

        result[2].IsActive.Should().BeFalse();
        result[2].Department.Should().Be("Sales");
    }

    /// <summary>
    /// Verifies that get all users should handle users with null departments correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllAsync_ShouldHandleNullDepartments_WhenRetrievingUsers()
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "User Without Department",
                Email = "nodept@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = new Role { Name = "User" },
                Department = null
            }
        };

        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var service = CreateService();
        var result = await service.GetAllAsync();

        result.Should().HaveCount(1);
        result[0].Department.Should().BeNull();
    }

    /// <summary>
    /// Verifies that get all users should use user repository to fetch users.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllAsync_ShouldCallRepository_WhenFetchingUsers()
    {
        var users = new List<User>();

        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var service = CreateService();
        await service.GetAllAsync();

        _userRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
