using CloudDocs.Application.Common.Models;
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
    /// Gets paged versions by document id.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of document version.</returns>
    public async Task<PagedResult<DocumentVersion>> GetPagedByDocumentIdAsync(Guid documentId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.DocumentVersions
            .Include(x => x.UploadedByUser)
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.VersionNumber)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DocumentVersion>
        {
            Items = items,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
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

    /// <summary>
    /// Retrieves a specific document version by its identifier.
    /// Includes related document, category, and user information for access validation.
    /// </summary>
    public async Task<DocumentVersion?> GetByIdAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentVersions
            .Include(x => x.Document)
                .ThenInclude(d => d.Category)
            .Include(x => x.Document)
                .ThenInclude(d => d.UploadedByUser)
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.Id == versionId, cancellationToken);
    }
}
