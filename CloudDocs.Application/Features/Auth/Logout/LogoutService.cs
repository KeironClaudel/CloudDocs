using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Auth.Logout;

public class LogoutService : ILogoutService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutService(
        IRefreshTokenRepository refreshTokenRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

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