using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for document version.
/// </summary>
public class DocumentVersionRepository : IDocumentVersionRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentVersionRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public DocumentVersionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(DocumentVersion version, CancellationToken cancellationToken = default)
    {
        await _context.DocumentVersions.AddAsync(version, cancellationToken);
    }

    /// <summary>
    /// Gets the item by document id.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document version list.</returns>
    public async Task<List<DocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentVersions
            .Include(x => x.UploadedByUser)
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the next version number.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the int.</returns>
    public async Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var lastVersion = await _context.DocumentVersions
            .Where(x => x.DocumentId == documentId)
            .MaxAsync(x => (int?)x.VersionNumber, cancellationToken);

        return (lastVersion ?? 0) + 1;
    }
}