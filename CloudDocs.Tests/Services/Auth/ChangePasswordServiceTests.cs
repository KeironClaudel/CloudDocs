using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Auth.ChangePassword;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Auth;

public class ChangePasswordServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private ChangePasswordService CreateService()
    {
        return new ChangePasswordService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("Old123!A", "NewPassword123!");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(userId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found or inactive.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserIsInactive()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("Old123!A", "NewPassword123!");

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            PasswordHash = "hashed",
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(userId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found or inactive.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenCurrentPasswordIsIncorrect()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("Wrong123!A", "NewPassword123!");

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            PasswordHash = "hashed",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.CurrentPassword, user.PasswordHash))
            .Returns(false);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(userId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Current password is incorrect.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenNewPasswordIsInvalid()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("Old123!A", "abc");

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            PasswordHash = "hashed",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.CurrentPassword, user.PasswordHash))
            .Returns(true);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(userId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("New password does not meet security requirements.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdatePassword_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("Old123!A", "NewPassword123!");

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            PasswordHash = "hashed",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.CurrentPassword, user.PasswordHash))
            .Returns(true);

        _passwordHasherMock
            .Setup(x => x.Hash(request.NewPassword))
            .Returns("new-hash");

        var service = CreateService();
        await service.ExecuteAsync(userId, request);

        user.PasswordHash.Should().Be("new-hash");
        user.UpdatedAt.Should().NotBeNull();

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}