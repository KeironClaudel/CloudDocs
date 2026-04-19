using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for user.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user when available; otherwise, null.</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(x => x.Role)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets the item by email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user when available; otherwise, null.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _context.Users
            .Include(x => x.Role)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user list.</returns>
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(x => x.Role)
            .Include(x => x.Department)
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Determines whether the email exists.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _context.Users
            .AnyAsync(x => x.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}
