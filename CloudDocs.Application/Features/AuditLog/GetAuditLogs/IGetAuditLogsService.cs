using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.AuditLogs.Common;

namespace CloudDocs.Application.Features.AuditLogs.GetAuditLogs;

/// <summary>
/// Defines the contract for get audit logs operations.
/// </summary>
public interface IGetAuditLogsService
{
    /// <summary>
    /// Gets.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="module">The module.</param>
    /// <param name="from">The from.</param>
    /// <param name="to">The to.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of audit log response.</returns>
    Task<PagedResult<AuditLogResponse>> GetAsync(
        Guid? userId,
        string? action,
        string? module,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}