using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.AuditLogs.Common;

namespace CloudDocs.Application.Features.AuditLogs.GetAuditLogs;

public interface IGetAuditLogsService
{
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