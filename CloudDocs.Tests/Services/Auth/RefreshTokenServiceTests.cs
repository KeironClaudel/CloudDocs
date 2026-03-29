using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Auth.RefreshToken;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RefreshTokenEntity = CloudDocs.Domain.Entities.RefreshToken;

namespace CloudDocs.Tests.Services.Auth;

/// <summary>
/// Contains tests for refresh token service.
/// </summary>
public class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RefreshTokenService CreateService()
    {
        return new RefreshTokenService(
            _refreshTokenRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokenGeneratorMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<RefreshTokenService>.Instance);
    }

    /// <summary>
    /// Verifies that execute async should throw bad request when refresh token is invalid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenRefreshTokenIsInvalid()
    {
        var request = new RefreshTokenRequest("invalid-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    /// <summary>
    /// Verifies that execute async should throw unauthorized when user is inactive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserIsInactive()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            FullName = "Inactive User",
            IsActive = false,
            Role = new Role { Name = "User" }
        };

        var existingToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = user
        };

        var request = new RefreshTokenRequest(existingToken.Token);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is inactive.");
    }

    /// <summary>
    /// Verifies that execute async should revoke old token and create new tokens when request is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldRevokeOldToken_AndCreateNewTokens_WhenRequestIsValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            FullName = "Normal User",
            IsActive = true,
            Role = new Role { Name = "Admin" }
        };

        var existingToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "old-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = user,
            RevokedAt = null
        };

        var request = new RefreshTokenRequest(existingToken.Token);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetValidTokenAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("new-refresh-token");

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user.Id, user.Email, user.Role.Name))
            .Returns("new-access-token");

        var service = CreateService();
        var result = await service.ExecuteAsync(request);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        existingToken.RevokedAt.Should().NotBeNull();

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<RefreshTokenEntity>(rt =>
                    rt.UserId == user.Id &&
                    rt.Token == "new-refresh-token" &&
                    rt.RevokedAt == null),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}