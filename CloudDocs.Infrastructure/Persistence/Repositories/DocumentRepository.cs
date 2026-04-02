using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for document.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document when available; otherwise, null.</returns>
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(x => x.Category)
            .Include(x => x.UploadedByUser)
            .Include(x => x.DocumentType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Searches.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of document.</returns>
    public async Task<PagedResult<Document>> SearchAsync(SearchDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Documents
                    .Include(x => x.Category)
                    .Include(x => x.DocumentType)
                    .Include(x => x.UploadedByUser)
                    .AsNoTracking()
                    .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim().ToLower();
            query = query.Where(x => x.OriginalFileName.ToLower().Contains(normalizedName));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);

        if (request.Month.HasValue)
            query = query.Where(x => x.Month == request.Month.Value);

        if (request.Year.HasValue)
            query = query.Where(x => x.Year == request.Year.Value);

        if (request.DocumentTypeId.HasValue)
            query = query.Where(x => x.DocumentTypeId == request.DocumentTypeId.Value);

        if (request.ExpirationPendingDefinition.HasValue)
            query = query.Where(x => x.ExpirationDatePendingDefinition == request.ExpirationPendingDefinition.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Document>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}