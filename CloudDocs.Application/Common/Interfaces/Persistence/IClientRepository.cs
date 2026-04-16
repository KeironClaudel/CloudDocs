using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

public interface IClientRepository
{
    Task<List<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Client>> SearchByNameAsync(string term, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IdentificationExistsAsync(string identification, CancellationToken cancellationToken = default);
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
}