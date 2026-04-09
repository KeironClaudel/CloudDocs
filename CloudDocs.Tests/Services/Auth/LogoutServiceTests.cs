using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Auth.Logout;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Auth;

/// <summary>
/// Contains tests for logout service.
/// </summary>
public class LogoutServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private LogoutService CreateService()
    {
        return new LogoutService(
            _refreshTokenRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that logout should revoke token and audit the logout successfully.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldRevokeToken_WhenValidTokenProvided()
    {
        var userId = Guid.NewGuid();
        var tokenValue = Guid.NewGuid().ToString("N");
        var request = new LogoutRequest(tokenValue);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RevokedAt = null
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(tokenValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var service = CreateService();
        await service.ExecuteAsync(request);

        refreshToken.RevokedAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                userId,
                "Logout",
                "Auth",
                "User",
                userId.ToString(),
                "User logged out and refresh token revoked.",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that logout should complete without error when token is invalid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldCompleteGracefully_WhenTokenIsInvalid()
    {
        var tokenValue = Guid.NewGuid().ToString("N");
        var request = new LogoutRequest(tokenValue);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(tokenValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(request);

        await act.Should().NotThrowAsync();
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _auditServiceMock.Verify(
            x => x.LogAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
