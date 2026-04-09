using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Auth.ForgotPassword;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CloudDocs.Tests.Services.Auth;

/// <summary>
/// Contains tests for forgot password service.
/// </summary>
public class ForgotPasswordServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();

    private ForgotPasswordService CreateService()
    {
        var frontendSettings = new Application.Common.Models.FrontendSettings
        {
            BaseUrl = "http://localhost:3000"
        };

        var optionsMock = new Mock<IOptions<Application.Common.Models.FrontendSettings>>();
        optionsMock.Setup(x => x.Value).Returns(frontendSettings);

        return new ForgotPasswordService(
            _userRepositoryMock.Object,
            _passwordResetTokenRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            optionsMock.Object);
    }

    /// <summary>
    /// Verifies that forgot password should generate token and send email successfully.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldGenerateTokenAndSendEmail_WhenActiveUserExists()
    {
        var request = new ForgotPasswordRequest("user@test.com");
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = request.Email,
            FullName = "Test User",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var result = await service.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.Message.Should().Contain("If the account exists");

        _passwordResetTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _emailServiceMock.Verify(
            x => x.SendAsync(
                request.Email,
                "CloudDocs Password Reset",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                userId,
                "ForgotPasswordRequested",
                "Auth",
                "User",
                userId.ToString(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that forgot password should return generic message when user does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnGenericMessage_WhenUserDoesNotExist()
    {
        var request = new ForgotPasswordRequest("nonexistent@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var result = await service.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.Message.Should().Contain("If the account exists");

        _passwordResetTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                null,
                "ForgotPasswordRequested",
                "Auth",
                "User",
                null,
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that forgot password should return generic message when user is inactive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldReturnGenericMessage_WhenUserIsInactive()
    {
        var request = new ForgotPasswordRequest("inactive@test.com");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = "Inactive User",
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var result = await service.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.Message.Should().Contain("If the account exists");

        _passwordResetTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
