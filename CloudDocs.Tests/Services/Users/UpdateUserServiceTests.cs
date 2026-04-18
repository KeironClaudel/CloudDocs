using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Users.UpdateUser;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Users;

/// <summary>
/// Contains tests for update user service.
/// </summary>
public class UpdateUserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private UpdateUserService CreateService()
    {
        return new UpdateUserService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _departmentRepositoryMock.Object,
            _passwordHasherMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that update user should return null when user does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var request = new UpdateUserRequest("New Name", "new@test.com", null, deptId, roleId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(userId, request);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that update user should throw exception when email is already in use by another user.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequest_WhenEmailAlreadyInUseByAnother()
    {
        var userId = Guid.NewGuid();
        var newEmail = "another@test.com";
        var roleId = Guid.NewGuid();
        var request = new UpdateUserRequest("New Name", newEmail, null, Guid.NewGuid(), roleId);

        var userToUpdate = new User
        {
            Id = userId,
            Email = "original@test.com",
            FullName = "Original Name",
            IsActive = true,
            Role = new Role { Name = "User" }
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = newEmail,
            FullName = "Another User",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userToUpdate);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(newEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var service = CreateService();
        var act = async () => await service.UpdateAsync(userId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Email is already in use.");
    }

    /// <summary>
    /// Verifies that update user should throw exception when role does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenRoleDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var request = new UpdateUserRequest("New Name", "new@test.com", null, Guid.NewGuid(), roleId);

        var user = new User
        {
            Id = userId,
            Email = "original@test.com",
            FullName = "Original Name",
            IsActive = true,
            Role = new Role { Name = "User" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _roleRepositoryMock
            .Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var service = CreateService();
        var act = async () => await service.UpdateAsync(userId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Role not found.");
    }

    /// <summary>
    /// Verifies that update user should update user successfully.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserSuccessfully_WhenAllDataIsValid()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var newEmail = "new@test.com";
        var newName = "New Name";
        var newDepartment = "Sales";
        var newDepartmentId = Guid.NewGuid();
        var request = new UpdateUserRequest(newName, newEmail, null, newDepartmentId, roleId);

        var user = new User
        {
            Id = userId,
            Email = "original@test.com",
            FullName = "Original Name",
            IsActive = true,
            Role = new Role { Name = "User" }
        };

        var role = new Role
        {
            Id = roleId,
            Name = "Admin"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(newEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _roleRepositoryMock
            .Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync(newDepartmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = newDepartmentId, Name = newDepartment, IsActive = true });

        var service = CreateService();
        var result = await service.UpdateAsync(userId, request);

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FullName.Should().Be(newName);
        result.Email.Should().Be(newEmail);
        result.Department.Should().Be(newDepartment);
        result.Role.Should().Be("Admin");

        _userRepositoryMock.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                null,
                "Update",
                "Users",
                "User",
                userId.ToString(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that update user should allow updating email to same email.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldAllowSameEmail_WhenUpdatingUser()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var email = "current@test.com";
        var request = new UpdateUserRequest("New Name", email, null, null, roleId);

        var user = new User
        {
            Id = userId,
            Email = email,
            FullName = "Original Name",
            IsActive = true,
            Role = new Role { Name = "User" }
        };

        var role = new Role { Id = roleId, Name = "Admin" };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleRepositoryMock
            .Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var service = CreateService();
        var result = await service.UpdateAsync(userId, request);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }
}
