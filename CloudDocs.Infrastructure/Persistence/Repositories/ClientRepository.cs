using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Client>> SearchByNameAsync(string term, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = term.Trim().ToLower();

        return await _context.Clients
            .AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(normalizedTerm))
            .OrderBy(x => x.Name)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.Clients
            .AnyAsync(x => x.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<bool> IdentificationExistsAsync(string identification, CancellationToken cancellationToken = default)
    {
        var normalizedIdentification = identification.Trim().ToLower();

        return await _context.Clients
            .AnyAsync(x => x.Identification != null &&
                           x.Identification.ToLower() == normalizedIdentification,
                cancellationToken);
    }

    public async Task AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        await _context.Clients.AddAsync(client, cancellationToken);
    }

    public Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Update(client);
        return Task.CompletedTask;
    }
}