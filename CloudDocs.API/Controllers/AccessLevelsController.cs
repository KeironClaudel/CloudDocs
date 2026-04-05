using CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.DeactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevel;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevelById;
using CloudDocs.Application.Features.AccessLevels.ReactivateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/access-levels")]
[Authorize]
public class AccessLevelsController : ControllerBase
{
    private readonly IGetAccessLevelsService _getAccessLevelsService;
    private readonly IGetAccessLevelByIdService _getAccessLevelByIdService;
    private readonly ICreateAccessLevelService _createAccessLevelService;
    private readonly IUpdateAccessLevelService _updateAccessLevelService;
    private readonly IDeactivateAccessLevelService _deactivateAccessLevelService;
    private readonly IReactivateAccessLevelService _reactivateAccessLevelService;

    public AccessLevelsController(
        IGetAccessLevelsService getAccessLevelsService,
        IGetAccessLevelByIdService getAccessLevelByIdService,
        ICreateAccessLevelService createAccessLevelService,
        IUpdateAccessLevelService updateAccessLevelService,
        IDeactivateAccessLevelService deactivateAccessLevelService,
        IReactivateAccessLevelService reactivateAccessLevelService)
    {
        _getAccessLevelsService = getAccessLevelsService;
        _getAccessLevelByIdService = getAccessLevelByIdService;
        _createAccessLevelService = createAccessLevelService;
        _updateAccessLevelService = updateAccessLevelService;
        _deactivateAccessLevelService = deactivateAccessLevelService;
        _reactivateAccessLevelService = reactivateAccessLevelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getAccessLevelsService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getAccessLevelByIdService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Access level not found." });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAccessLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _createAccessLevelService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccessLevelRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateAccessLevelService.UpdateAsync(id, request, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Access level not found." });

        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateAccessLevelService.DeactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Access level not found." });

        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateAccessLevelService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Access level not found." });

        return NoContent();
    }
}