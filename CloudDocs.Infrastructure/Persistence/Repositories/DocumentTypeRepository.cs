using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for document type entities.
/// </summary>
public class DocumentTypeRepository : IDocumentTypeRepository
{
    private readonly AppDbContext _context;

    public DocumentTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentTypeEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentTypeEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTypes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.DocumentTypes
            .AnyAsync(x => x.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task AddAsync(DocumentTypeEntity documentType, CancellationToken cancellationToken = default)
    {
        await _context.DocumentTypes.AddAsync(documentType, cancellationToken);
    }

    public Task UpdateAsync(DocumentTypeEntity documentType, CancellationToken cancellationToken = default)
    {
        _context.DocumentTypes.Update(documentType);
        return Task.CompletedTask;
    }
}