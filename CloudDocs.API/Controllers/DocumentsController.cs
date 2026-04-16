using CloudDocs.API.Contracts.Documents;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Application.Features.Documents.DeactivateDocument;
using CloudDocs.Application.Features.Documents.GetDocumentById;
using CloudDocs.Application.Features.Documents.GetDocumentFile;
using CloudDocs.Application.Features.Documents.ReactivateDocument;
using CloudDocs.Application.Features.Documents.RenameDocument;
using CloudDocs.Application.Features.Documents.SearchDocuments;
using CloudDocs.Application.Features.Documents.SendDocumentToClient;
using CloudDocs.Application.Features.Documents.UpdateDocumentVisibility;
using CloudDocs.Application.Features.Documents.UploadDocument;
using CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;
using CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudDocs.API.Controllers;

/// <summary>
/// Exposes endpoints for documents.
/// </summary>
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
    private readonly IUpdateDocumentVisibilityService _updateDocumentVisibilityService;
    private readonly IUserRepository _userRepository;
    private readonly ISendDocumentToClientService _sendDocumentToClientService;
    private readonly IReactivateDocumentService _reactivateDocumentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentsController"/> class.
    /// </summary>
    /// <param name="uploadDocumentService">The upload document service.</param>
    /// <param name="searchDocumentsService">The search documents service.</param>
    /// <param name="getDocumentByIdService">The get document by id service.</param>
    /// <param name="renameDocumentService">The rename document service.</param>
    /// <param name="deactivateDocumentService">The deactivate document service.</param>
    /// <param name="getDocumentFileService">The get document file service.</param>
    /// <param name="getDocumentVersionsService">The get document versions service.</param>
    /// <param name="uploadDocumentVersionService">The upload document version service.</param>
    /// <param name="updateDocumentVisibilityService">The update document visibility service.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="sendDocumentToClientService">The send document to client service.</param>
    /// <param name="reactivateDocumentService">The reactivate document service.</param>
    public DocumentsController(
    IUploadDocumentService uploadDocumentService,
    ISearchDocumentsService searchDocumentsService,
    IGetDocumentByIdService getDocumentByIdService,
    IRenameDocumentService renameDocumentService,
    IDeactivateDocumentService deactivateDocumentService,
    IGetDocumentFileService getDocumentFileService,
    IGetDocumentVersionsService getDocumentVersionsService,
    IUploadDocumentVersionService uploadDocumentVersionService,
    IUpdateDocumentVisibilityService updateDocumentVisibilityService,
    IUserRepository userRepository,
    ISendDocumentToClientService sendDocumentToClientService,
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
        _updateDocumentVisibilityService = updateDocumentVisibilityService;
        _userRepository = userRepository;
        _sendDocumentToClientService = sendDocumentToClientService;
        _reactivateDocumentService = reactivateDocumentService;
    }

    /// <summary>
    /// Uploads.
    /// </summary>
    /// <param name="form">The form data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
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
                form.DocumentTypeId,
                form.ExpirationDate,
                form.ExpirationDatePendingDefinition,
                form.AccessLevelId,
                form.ClientId,
                form.DepartmentIds);

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

    /// <summary>
    /// Searches.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="categoryId">The category id identifier.</param>
    /// <param name="month">The month.</param>
    /// <param name="year">The year.</param>
    /// <param name="documentType">The document type.</param>
    /// <param name="expirationPendingDefinition">The expiration pending definition.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="includeInactive">The include inactive.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet] 
    public async Task<IActionResult> Search(
    [FromQuery] string? name,
    [FromQuery] Guid? categoryId,
    [FromQuery] Guid? clientId,
    [FromQuery] int? month,
    [FromQuery] int? year,
    [FromQuery] Guid? documentTypeId,
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
            clientId,
            month,
            year,
            documentTypeId,
            expirationPendingDefinition,
            includeInactive,
            page,
            pageSize);

        var result = await _searchDocumentsService.SearchAsync(currentUser, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
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

    /// <summary>
    /// Previews.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// /// <param name="versionId">The documents version identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(
    Guid id,
    [FromQuery] Guid? versionId = null,
    CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var file = await _getDocumentFileService.GetFileAsync(
            currentUser,
            id,
            "Preview",
            userId,
            versionId,
            cancellationToken);

        if (file is null)
            return NotFound(new { message = "Document file not found or access denied." });

        return File(file.Value.Stream!, file.Value.ContentType);
    }

    /// <summary>
    /// Downloads.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// /// <param name="versionId">The document version identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(
    Guid id,
    [FromQuery] Guid? versionId = null,
    CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (currentUser is null)
            return Unauthorized(new { message = "User not found." });

        var file = await _getDocumentFileService.GetFileAsync(
            currentUser,
            id,
            "Download",
            userId,
            versionId,
            cancellationToken);

        if (file is null)
            return NotFound(new { message = "Document file not found or access denied." });

        return File(file.Value.Stream!, file.Value.ContentType, file.Value.FileName);
    }

    /// <summary>
    /// Renames.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
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

    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateDocumentService.DeactivateAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "Document not found." });

        return NoContent();
    }

    /// <summary>
    /// Gets the versions.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
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

    /// <summary>
    /// Uploads the version.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="form">The form data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
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

    /// <summary>
    /// Reactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateDocumentService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Document not found." });

        return NoContent();
    }

    /// <summary>
    /// Updates document visibility.
    /// </summary>
    /// <param name="id"> Theidentifier.</param>
    /// <param name="request"> The request data. </param>
    /// <param name="cancellationToken"> The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/visibility")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVisibility(
    Guid id,
    [FromBody] UpdateDocumentVisibilityRequest request,
    CancellationToken cancellationToken)
    {
        var success = await _updateDocumentVisibilityService.UpdateAsync(id, request, cancellationToken);

        if (!success)
            return NotFound(new { message = "Document not found." });

        return NoContent();
    }

    /// <summary>
    /// Sends a document to a client.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("{id:guid}/send-to-client")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendToClient(
    Guid id,
    [FromBody] SendDocumentToClientRequest request,
    CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        await _sendDocumentToClientService.ExecuteAsync(
            id,
            userId,
            request,
            cancellationToken);

        return NoContent();
    }
}