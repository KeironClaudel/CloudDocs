using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Auth.ResetPassword;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Auth;

/// <summary>
/// Contains tests for reset password service.
/// </summary>
public class ResetPasswordServiceTests
{
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private ResetPasswordService CreateService()
    {
        return new ResetPasswordService(
            _passwordResetTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that reset password should throw exception when token is invalid or expired.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenTokenIsInvalidOrExpired()
    {
        var request = new ResetPasswordRequest("invalid-token", "NewPassword123!");

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid or expired token.");
    }

    /// <summary>
    /// Verifies that reset password should update user password and mark token as used successfully.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldUpdatePasswordAndMarkTokenAsUsed_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var newPassword = "NewPassword123!";
        var hashedPassword = "hashed-new-password";
        var tokenValue = Guid.NewGuid().ToString("N");
        var request = new ResetPasswordRequest(tokenValue, newPassword);

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FullName = "Test User",
            PasswordHash = "old-hash",
            IsActive = true
        };

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            User = user,
            UsedAt = null
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resetToken);

        _passwordHasherMock
            .Setup(x => x.Hash(newPassword))
            .Returns(hashedPassword);

        var service = CreateService();
        await service.ExecuteAsync(request);

        user.PasswordHash.Should().Be(hashedPassword);
        resetToken.UsedAt.Should().NotBeNull();

        _userRepositoryMock.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                userId,
                "PasswordResetCompleted",
                "Auth",
                "User",
                userId.ToString(),
                "Password reset completed successfully.",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that reset password uses password hasher to hash the new password.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldUsePasswordHasher_WhenHashingNewPassword()
    {
        var userId = Guid.NewGuid();
        var newPassword = "NewPassword123!";
        var hashedPassword = "hashed-new-password";
        var tokenValue = Guid.NewGuid().ToString("N");
        var request = new ResetPasswordRequest(tokenValue, newPassword);

        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            FullName = "Test User",
            PasswordHash = "old-hash",
            IsActive = true
        };

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            User = user,
            UsedAt = null
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resetToken);

        _passwordHasherMock
            .Setup(x => x.Hash(newPassword))
            .Returns(hashedPassword);

        var service = CreateService();
        await service.ExecuteAsync(request);

        _passwordHasherMock.Verify(
            x => x.Hash(newPassword),
            Times.Once);
    }
}
