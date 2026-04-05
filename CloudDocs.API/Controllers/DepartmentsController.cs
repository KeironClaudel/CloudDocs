using CloudDocs.Application.Features.Departments.CreateDepartment;
using CloudDocs.Application.Features.Departments.DeactivateDepartment;
using CloudDocs.Application.Features.Departments.GetDepartmentById;
using CloudDocs.Application.Features.Departments.GetDepartments;
using CloudDocs.Application.Features.Departments.ReactivateDepartment;
using CloudDocs.Application.Features.Departments.UpdateDepartment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IGetDepartmentsService _getDepartmentsService;
    private readonly IGetDepartmentByIdService _getDepartmentByIdService;
    private readonly ICreateDepartmentService _createDepartmentService;
    private readonly IUpdateDepartmentService _updateDepartmentService;
    private readonly IDeactivateDepartmentService _deactivateDepartmentService;
    private readonly IReactivateDepartmentService _reactivateDepartmentService;

    public DepartmentsController(
        IGetDepartmentsService getDepartmentsService,
        IGetDepartmentByIdService getDepartmentByIdService,
        ICreateDepartmentService createDepartmentService,
        IUpdateDepartmentService updateDepartmentService,
        IDeactivateDepartmentService deactivateDepartmentService,
        IReactivateDepartmentService reactivateDepartmentService)
    {
        _getDepartmentsService = getDepartmentsService;
        _getDepartmentByIdService = getDepartmentByIdService;
        _createDepartmentService = createDepartmentService;
        _updateDepartmentService = updateDepartmentService;
        _deactivateDepartmentService = deactivateDepartmentService;
        _reactivateDepartmentService = reactivateDepartmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getDepartmentsService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getDepartmentByIdService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Department not found." });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _createDepartmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateDepartmentService.UpdateAsync(id, request, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Department not found." });

        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateDepartmentService.DeactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Department not found." });

        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateDepartmentService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Department not found." });

        return NoContent();
    }
}