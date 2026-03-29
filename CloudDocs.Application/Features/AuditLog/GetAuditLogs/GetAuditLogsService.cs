using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.AuditLogs.Common;

namespace CloudDocs.Application.Features.AuditLogs.GetAuditLogs;

/// <summary>
/// Provides operations for get audit logs.
/// </summary>
public class GetAuditLogsService : IGetAuditLogsService
{
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuditLogsService"/> class.
    /// </summary>
    /// <param name="auditLogRepository">The audit log repository.</param>
    public GetAuditLogsService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

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
    public async Task<PagedResult<AuditLogResponse>> GetAsync(
        Guid? userId,
        string? action,
        string? module,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _auditLogRepository.SearchAsync(
            userId, action, module, from, to, page, pageSize, cancellationToken);

        return new PagedResult<AuditLogResponse>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new AuditLogResponse(
                x.Id,
                x.UserId,
                x.Action,
                x.Module,
                x.EntityName,
                x.EntityId,
                x.Details,
                x.IpAddress,
                x.CreatedAt)).ToList()
        };
    }
}