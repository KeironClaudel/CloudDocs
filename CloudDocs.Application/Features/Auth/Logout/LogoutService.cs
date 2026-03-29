using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Auth.Logout;

/// <summary>
/// Provides operations for logout.
/// </summary>
public class LogoutService : ILogoutService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutService"/> class.
    /// </summary>
    /// <param name="refreshTokenRepository">The refresh token repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public LogoutService(
        IRefreshTokenRepository refreshTokenRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenRepository.GetValidTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken is null)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            refreshToken.UserId,
            "Logout",
            "Auth",
            "User",
            refreshToken.UserId.ToString(),
            "User logged out and refresh token revoked.",
            null,
            cancellationToken);
    }
}