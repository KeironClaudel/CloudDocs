using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for access level entities.
/// </summary>
public class AccessLevelRepository : IAccessLevelRepository
{
    private readonly AppDbContext _context;

    public AccessLevelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccessLevelEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AccessLevels
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<AccessLevelEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AccessLevels
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AccessLevelEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.AccessLevels
            .FirstOrDefaultAsync(x => x.Code.ToUpper() == normalized, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.AccessLevels
            .AnyAsync(x => x.Code.ToUpper() == normalized, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        return await _context.AccessLevels
            .AnyAsync(x => x.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(AccessLevelEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.AccessLevels.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(AccessLevelEntity entity, CancellationToken cancellationToken = default)
    {
        _context.AccessLevels.Update(entity);
        return Task.CompletedTask;
    }
}
