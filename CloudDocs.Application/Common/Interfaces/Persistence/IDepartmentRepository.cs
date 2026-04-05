using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Department>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
