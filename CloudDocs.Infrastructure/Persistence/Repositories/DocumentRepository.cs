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
                        .Include(x => x.Client)
                        .Include(x => x.DocumentType)
                        .Include(x => x.AccessLevel)
                        .Include(x => x.UploadedByUser)
                        .Include(x => x.DocumentDepartments)
                            .ThenInclude(dd => dd.Department)
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
    public async Task<PagedResult<Document>> SearchAsync(User currentUser, SearchDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        var isAdmin = string.Equals(currentUser.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _context.Documents
            .AsNoTracking()
            .AsSplitQuery()
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!isAdmin)
        {
            var departmentId = currentUser.DepartmentId;

            query = query.Where(x =>
                x.AccessLevel.Code == "INTERNAL_PUBLIC" ||
                (x.AccessLevel.Code == "OWNER_ONLY" && x.UploadedByUserId == currentUser.Id) ||
                (x.AccessLevel.Code == "DEPARTMENT_ONLY" &&
                 departmentId.HasValue &&
                 x.DocumentDepartments.Any(dd => dd.DepartmentId == departmentId.Value)));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var pattern = $"%{request.Name.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.OriginalFileName, pattern));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);

        if (request.ClientId.HasValue)
            query = query.Where(x => x.ClientId == request.ClientId.Value);

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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Category)
            .Include(x => x.Client)
            .Include(x => x.DocumentType)
            .Include(x => x.AccessLevel)
            .Include(x => x.UploadedByUser)
            .Include(x => x.DocumentDepartments)
                .ThenInclude(dd => dd.Department)
            .ToListAsync(cancellationToken);

        return new PagedResult<Document>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Counts documents by user.
    /// </summary>
    /// <param name="uploadedByUserId">The identifier of the user who uploaded the documents.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of documents uploaded by the specified user.</returns>
    public async Task<int> CountByUserAsync(Guid uploadedByUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .CountAsync(x => x.UploadedByUserId == uploadedByUserId && x.IsActive, cancellationToken);
    }
}
