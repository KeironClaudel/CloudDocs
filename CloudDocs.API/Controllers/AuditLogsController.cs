using CloudDocs.Application.Features.AuditLogs.GetAuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IGetAuditLogsService _getAuditLogsService;

    public AuditLogsController(IGetAuditLogsService getAuditLogsService)
    {
        _getAuditLogsService = getAuditLogsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? module,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _getAuditLogsService.GetAsync(
            userId, action, module, from, to, page, pageSize, cancellationToken);

        return Ok(result);
    }
}