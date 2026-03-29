using CloudDocs.Application.Common.Models;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for audit log persistence operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Searches.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="module">The module.</param>
    /// <param name="from">The from.</param>
    /// <param name="to">The to.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of audit log.</returns>
    Task<PagedResult<AuditLog>> SearchAsync(
        Guid? userId,
        string? action,
        string? module,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}