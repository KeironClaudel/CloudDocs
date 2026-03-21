namespace CloudDocs.Application.Features.Auth.RefreshToken;

public interface IRefreshTokenService
{
    Task<RefreshTokenResponse> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}