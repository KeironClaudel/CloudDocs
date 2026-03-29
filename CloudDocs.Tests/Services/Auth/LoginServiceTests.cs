using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Auth.Login;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RefreshTokenEntity = CloudDocs.Domain.Entities.RefreshToken;

namespace CloudDocs.Tests.Services.Auth;

/// <summary>
/// Contains tests for login service.
/// </summary>
public class LoginServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private LoginService CreateService()
    {
        return new LoginService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _auditServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _refreshTokenGeneratorMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<LoginService>.Instance);
    }

    /// <summary>
    /// Verifies that login async should throw unauthorized when user does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserDoesNotExist()
    {
        var request = new LoginRequest("missing@test.com", "Password123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var act = async () => await service.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    /// <summary>
    /// Verifies that login async should throw unauthorized when user is inactive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserIsInactive()
    {
        var request = new LoginRequest("inactive@test.com", "Password123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = "Inactive User",
            PasswordHash = "hashed",
            IsActive = false,
            Role = new Role { Name = "User" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var act = async () => await service.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is inactive.");
    }

    /// <summary>
    /// Verifies that login async should throw unauthorized when user is locked.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserIsLocked()
    {
        var request = new LoginRequest("locked@test.com", "Password123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = "Locked User",
            PasswordHash = "hashed",
            IsActive = true,
            LockoutEndUtc = DateTime.UtcNow.AddMinutes(10),
            Role = new Role { Name = "User" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var act = async () => await service.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is temporarily locked.");
    }

    /// <summary>
    /// Verifies that login async should throw unauthorized when password is invalid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        var request = new LoginRequest("user@test.com", "WrongPassword123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = "Normal User",
            PasswordHash = "hashed",
            IsActive = true,
            FailedLoginAttempts = 0,
            Role = new Role { Name = "User" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        var service = CreateService();
        var act = async () => await service.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");

        user.FailedLoginAttempts.Should().Be(1);

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that login async should lock user when password fails five times.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LoginAsync_ShouldLockUser_WhenPasswordFailsFiveTimes()
    {
        var request = new LoginRequest("user@test.com", "WrongPassword123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = "Normal User",
            PasswordHash = "hashed",
            IsActive = true,
            FailedLoginAttempts = 4,
            Role = new Role { Name = "User" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        var service = CreateService();
        var act = async () => await service.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>();

        user.LockoutEndUtc.Should().NotBeNull();
        user.FailedLoginAttempts.Should().Be(0);
    }

    /// <summary>
    /// Verifies that login async should return tokens when credentials are valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var request = new LoginRequest("admin@test.com", "Password123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = "Admin User",
            PasswordHash = "hashed",
            IsActive = true,
            FailedLoginAttempts = 2,
            Role = new Role { Name = "Admin" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user.Id, user.Email, user.Role.Name))
            .Returns("access-token");

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("refresh-token");

        var service = CreateService();
        var result = await service.LoginAsync(request);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Email.Should().Be(user.Email);
        result.FullName.Should().Be(user.FullName);
        result.Role.Should().Be("Admin");

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEndUtc.Should().BeNull();
        user.LastActivityAtUtc.Should().NotBeNull();

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.Is<RefreshTokenEntity>(r =>
                r.UserId == user.Id &&
                r.Token == "refresh-token"), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}