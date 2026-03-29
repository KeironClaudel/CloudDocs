namespace CloudDocs.Application.Features.Auth.RefreshToken;

/// <summary>
/// Defines the contract for refresh token operations.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token response.</returns>
    Task<RefreshTokenResponse> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}