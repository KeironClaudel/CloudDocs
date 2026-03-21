using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.AuditLogs.Common;

namespace CloudDocs.Application.Features.AuditLogs.GetAuditLogs;

public class GetAuditLogsService : IGetAuditLogsService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

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