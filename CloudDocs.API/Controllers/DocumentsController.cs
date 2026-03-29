using System.Security.Claims;
using CloudDocs.API.Contracts.Documents;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Application.Features.Documents.DeactivateDocument;
using CloudDocs.Application.Features.Documents.GetDocumentById;
using CloudDocs.Application.Features.Documents.GetDocumentFile;
using CloudDocs.Application.Features.Documents.RenameDocument;
using CloudDocs.Application.Features.Documents.SearchDocuments;
using CloudDocs.Application.Features.Documents.UploadDocument;
using CloudDocs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;
using CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Documents.ReactivateDocument;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IUploadDocumentService _uploadDocumentService;
    private readonly ISearchDocumentsService _searchDocumentsService;
    private readonly IGetDocumentByIdService _getDocumentByIdService;
    private readonly IRenameDocumentService _renameDocumentService;
    private readonly IDeactivateDocumentService _deactivateDocumentService;
    private readonly IGetDocumentFileService _getDocumentFileService;
    private readonly IGetDocumentVersionsService _getDocumentVersionsService;
    private readonly IUploadDocumentVersionService _uploadDocumentVersionService;
    private readonly IUserRepository _userRepository;
    private readonly IReactivateDocumentService _reactivateDocumentService;

    public DocumentsController(
    IUploadDocumentService uploadDocumentService,
    ISearchDocumentsService searchDocumentsService,
    IGetDocumentByIdService getDocumentByIdService,
    IRenameDocumentService renameDocumentService,
    IDeactivateDocumentService deactivateDocumentService,
    IGetDocumentFileService getDocumentFileService,
    IGetDocumentVersionsService getDocumentVersionsService,
    IUploadDocumentVersionService uploadDocumentVersionService,
    IUserRepository userRepository,
    IReactivateDocumentService reactivateDocumentService    )
    {
        _uploadDocumentService = uploadDocumentService;
        _searchDocumentsService = searchDocumentsService;
        _getDocumentByIdService = getDocumentByIdService;
        _renameDocumentService = renameDocumentService;
        _deactivateDocumentService = deactivateDocumentService;
        _getDocumentFileService = getDocumentFileService;
        _getDocumentVersionsService = getDocumentVersionsService;
        _uploadDocumentVersionService = uploadDocumentVersionService;
        _userRepository = userRepository;
        _reactivateDocumentService = reactivateDocumentService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentFormRequest form, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user token." });

            var request = new UploadDocumentRequest(
                form.File.FileName,
                form.File.ContentType,
                form.File.Length,
                form.CategoryId,
                form.DocumentType,
                form.ExpirationDate,
                form.ExpirationDatePendingDefinition,
                form.AccessLevel,
                form.Department);

            await using var stream = form.File.OpenReadStream();

            var created = await _uploadDocumentService.UploadAsync(
                userId,
                stream,
                request,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet] 
    public async Task<IActionResult> Search(
    [FromQuery] string? name,
    [FromQuery] Guid? categoryId,
    [FromQuery] int? month,
    [FromQuery] int? year,
    [FromQuery] DocumentType? documentType,
    [FromQuery] bool? expirationPendingDefinition,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] bool includeInactive = false,
    CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var isAdmin = string.Equals(currentUser.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase);

        if (includeInactive && !isAdmin)
        {
            return Forbid();
        }

        var request = new SearchDocumentsRequest(
            name,
            categoryId,
            month,
            year,
            documentType,
            expirationPendingDefinition,
            includeInactive,
            page,
            pageSize);

        var result = await _searchDocumentsService.SearchAsync(currentUser, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var document = await _getDocumentByIdService.GetByIdAsync(currentUser, id, cancellationToken);
        if (document is null)
            return NotFound(new { message = "Document not found or access denied." });

        return Ok(document);
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var file = await _getDocumentFileService.GetFileAsync(currentUser, id, "Preview", userId, cancellationToken);
        if (file is null)
            return NotFound(new { message = "Document file not found or access denied." });

        return File(file.Value.Stream!, file.Value.ContentType);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var file = await _getDocumentFileService.GetFileAsync(currentUser, id, "Download", userId, cancellationToken);
        if (file is null)
            return NotFound(new { message = "Document file not found or access denied." });

        return File(file.Value.Stream!, file.Value.ContentType, file.Value.FileName);
    }

    [HttpPut("{id:guid}/rename")]
    public async Task<IActionResult> Rename(Guid id, [FromBody] RenameDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _renameDocumentService.RenameAsync(id, request, cancellationToken);

            if (!success)
                return NotFound(new { message = "Document not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateDocumentService.DeactivateAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "Document not found." });

        return NoContent();
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var document = await _getDocumentByIdService.GetByIdAsync(currentUser, id, cancellationToken);
        if (document is null)
            return NotFound(new { message = "Document not found or access denied." });

        var versions = await _getDocumentVersionsService.GetByDocumentIdAsync(id, cancellationToken);

        return Ok(versions);
    }

    [HttpPost("{id:guid}/versions")]
    public async Task<IActionResult> UploadVersion(Guid id, [FromForm] UploadDocumentVersionFormRequest form, CancellationToken cancellationToken)
    {

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var existingDocument = await _getDocumentByIdService.GetByIdAsync(currentUser, id, cancellationToken);
        if (existingDocument is null)
            return NotFound(new { message = "Document not found or access denied." });

        var request = new UploadDocumentVersionRequest(
            form.File.FileName,
            form.File.ContentType,
            form.File.Length);

        await using var stream = form.File.OpenReadStream();

        var created = await _uploadDocumentVersionService.UploadAsync(
            id,
            userId,
            stream,
            request,
            cancellationToken);

        return Ok(created);
    }

    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateDocumentService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Document not found." });

        return NoContent();
    }
}