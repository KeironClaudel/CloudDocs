using CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.DeactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypeById;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypes;
using CloudDocs.Application.Features.DocumentTypes.ReactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/document-types")]
[Authorize]
public class DocumentTypesController : ControllerBase
{
    private readonly IGetDocumentTypesService _getDocumentTypesService;
    private readonly IGetDocumentTypeByIdService _getDocumentTypeByIdService;
    private readonly ICreateDocumentTypeService _createDocumentTypeService;
    private readonly IUpdateDocumentTypeService _updateDocumentTypeService;
    private readonly IDeactivateDocumentTypeService _deactivateDocumentTypeService;
    private readonly IReactivateDocumentTypeService _reactivateDocumentTypeService;

    public DocumentTypesController(
        IGetDocumentTypesService getDocumentTypesService,
        IGetDocumentTypeByIdService getDocumentTypeByIdService,
        ICreateDocumentTypeService createDocumentTypeService,
        IUpdateDocumentTypeService updateDocumentTypeService,
        IDeactivateDocumentTypeService deactivateDocumentTypeService,
        IReactivateDocumentTypeService reactivateDocumentTypeService)
    {
        _getDocumentTypesService = getDocumentTypesService;
        _getDocumentTypeByIdService = getDocumentTypeByIdService;
        _createDocumentTypeService = createDocumentTypeService;
        _updateDocumentTypeService = updateDocumentTypeService;
        _deactivateDocumentTypeService = deactivateDocumentTypeService;
        _reactivateDocumentTypeService = reactivateDocumentTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getDocumentTypesService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getDocumentTypeByIdService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Document type not found." });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDocumentTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _createDocumentTypeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateDocumentTypeService.UpdateAsync(id, request, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Document type not found." });

        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateDocumentTypeService.DeactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Document type not found." });

        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateDocumentTypeService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Document type not found." });

        return NoContent();
    }
}