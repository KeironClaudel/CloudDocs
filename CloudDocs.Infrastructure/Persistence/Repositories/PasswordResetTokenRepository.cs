using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for password reset token.
/// </summary>
public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetTokenRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
    }

    /// <summary>
    /// Gets the valid token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the password reset token when available; otherwise, null.</returns>
    public async Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Token == token &&
                x.UsedAt == null &&
                x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }
}