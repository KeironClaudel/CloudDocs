using CloudDocs.Application.Features.AuditLogs.GetAuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

/// <summary>
/// Exposes endpoints for audit logs.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IGetAuditLogsService _getAuditLogsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogsController"/> class.
    /// </summary>
    /// <param name="getAuditLogsService">The get audit logs service.</param>
    public AuditLogsController(IGetAuditLogsService getAuditLogsService)
    {
        _getAuditLogsService = getAuditLogsService;
    }

    /// <summary>
    /// Return the audit logs to the frontend.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="module">The module.</param>
    /// <param name="from">The from.</param>
    /// <param name="to">The to.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
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