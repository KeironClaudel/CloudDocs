using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for password reset token persistence operations.
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the valid token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the password reset token when available; otherwise, null.</returns>
    Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
}