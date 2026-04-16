using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

public class SentEmailLogRepository : ISentEmailLogRepository
{
    private readonly AppDbContext _context;

    public SentEmailLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SentEmailLogs
            .CountAsync(x => x.SentByUserId == userId, cancellationToken);
    }

    public async Task AddAsync(SentEmailLog sentEmailLog, CancellationToken cancellationToken = default)
    {
        await _context.SentEmailLogs.AddAsync(sentEmailLog, cancellationToken);
    }
}