using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = CloudDocs.Domain.Entities.RefreshToken;

namespace CloudDocs.Application.Features.Auth.RefreshToken;

/// <summary>
/// Provides operations for refresh token.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IAuditService _auditService;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuthCookieSettings _authCookieSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenService"/> class.
    /// </summary>
    /// <param name="refreshTokenRepository">The refresh token repository.</param>
    /// <param name="jwtTokenGenerator">The jwt token generator.</param>
    /// <param name="refreshTokenGenerator">The refresh token generator.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IOptions<AuthCookieSettings> authCookieOptions,
        ILogger<RefreshTokenService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _authCookieSettings = authCookieOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token response.</returns>
    public async Task<RefreshTokenResponse> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var existingRefreshToken = await _refreshTokenRepository.GetValidTokenAsync(request.RefreshToken, cancellationToken);

        if (existingRefreshToken is null)
        {
            _logger.LogWarning("Refresh token attempt failed. Invalid or expired token.");
            throw new BadRequestException("Invalid or expired refresh token.");
        }

        var user = existingRefreshToken.User;

        if (!user.IsActive)
            throw new UnauthorizedException("User is inactive.");

        existingRefreshToken.RevokedAt = DateTime.UtcNow;
        var newRefreshTokenValue = _refreshTokenGenerator.Generate();

        var newRefreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _authCookieSettings.RefreshTokenDays > 0 ? _authCookieSettings.RefreshTokenDays : 7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.Name);

        await _auditService.LogAsync(
            user.Id,
            "RefreshTokenUsed",
            "Auth",
            "User",
            user.Id.ToString(),
            "Access token refreshed successfully.",
            null,
            cancellationToken);

        _logger.LogInformation("Refresh token used successfully for user {UserId}.", user.Id);

        return new RefreshTokenResponse(newAccessToken, newRefreshTokenValue);
    }
}
