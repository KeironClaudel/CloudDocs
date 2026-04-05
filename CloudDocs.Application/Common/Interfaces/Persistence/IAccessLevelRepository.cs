using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

public interface IAccessLevelRepository
{
    Task<List<AccessLevelEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AccessLevelEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccessLevelEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(AccessLevelEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccessLevelEntity entity, CancellationToken cancellationToken = default);
}