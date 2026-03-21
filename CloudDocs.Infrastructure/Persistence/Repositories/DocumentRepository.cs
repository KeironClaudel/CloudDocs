using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(x => x.Category)
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<Document>> SearchAsync(SearchDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Documents
                    .Include(x => x.Category)
                    .Include(x => x.UploadedByUser)
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .AsQueryable();

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

        if (request.DocumentType.HasValue)
            query = query.Where(x => x.DocumentType == request.DocumentType.Value);

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