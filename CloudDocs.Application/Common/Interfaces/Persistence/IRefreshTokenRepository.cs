using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for refresh token persistence operations.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the valid token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token when available; otherwise, null.</returns>
    Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
}