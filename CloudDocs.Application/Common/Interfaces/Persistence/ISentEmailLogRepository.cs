using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

public interface ISentEmailLogRepository
{
    Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(SentEmailLog sentEmailLog, CancellationToken cancellationToken = default);
}