using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

public class DocumentVersionRepository : IDocumentVersionRepository
{
    private readonly AppDbContext _context;

    public DocumentVersionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DocumentVersion version, CancellationToken cancellationToken = default)
    {
        await _context.DocumentVersions.AddAsync(version, cancellationToken);
    }

    public async Task<List<DocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentVersions
            .Include(x => x.UploadedByUser)
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var lastVersion = await _context.DocumentVersions
            .Where(x => x.DocumentId == documentId)
            .MaxAsync(x => (int?)x.VersionNumber, cancellationToken);

        return (lastVersion ?? 0) + 1;
    }
}