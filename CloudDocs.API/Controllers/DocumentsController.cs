using CloudDocs.API.Common;
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
using Microsoft.AspNetCore.RateLimiting;
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(CancellationToken cancellationToken)
    {
        try
        {
            await using var multipart = await MultipartRequestReader.ReadAsync(Request, cancellationToken);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user token." });

            if (multipart.File is null)
                return BadRequest(new { message = "File is required." });

            var request = new UploadDocumentRequest(
                multipart.File.FileName,
                multipart.File.ContentType ?? "application/pdf",
                multipart.File.Length,
                GetRequiredGuid(multipart, nameof(UploadDocumentFormRequest.CategoryId)),
                GetRequiredGuid(multipart, nameof(UploadDocumentFormRequest.DocumentTypeId)),
                GetOptionalDateTime(multipart, nameof(UploadDocumentFormRequest.ExpirationDate)),
                GetBoolean(multipart, nameof(UploadDocumentFormRequest.ExpirationDatePendingDefinition)),
                GetRequiredGuid(multipart, nameof(UploadDocumentFormRequest.AccessLevelId)),
                GetRequiredGuid(multipart, nameof(UploadDocumentFormRequest.ClientId)),
                GetGuidList(multipart, nameof(UploadDocumentFormRequest.DepartmentIds)));

            await using var stream = multipart.File.OpenReadStream();

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
        catch (FormatException ex)
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
    /// <param name="versionId">The document version identifier.</param>
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

        return new FileStreamResult(file.Value.Stream!, file.Value.ContentType)
        {
            EnableRangeProcessing = true
        };
    }

    /// <summary>
    /// Downloads.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="versionId">The document version identifier.</param>
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

        return new FileStreamResult(file.Value.Stream!, file.Value.ContentType)
        {
            FileDownloadName = file.Value.FileName,
            EnableRangeProcessing = true
        };
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
    /// Gets paged versions for a document.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet("{id:guid}/versions/paged")]
    public async Task<IActionResult> GetVersionsPaged(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
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

        var versions = await _getDocumentVersionsService.GetPagedByDocumentIdAsync(id, page, pageSize, cancellationToken);

        return Ok(versions);
    }

    /// <summary>
    /// Uploads the version.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("{id:guid}/versions")]
    public async Task<IActionResult> UploadVersion(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await using var multipart = await MultipartRequestReader.ReadAsync(Request, cancellationToken);
            if (multipart.File is null)
                return BadRequest(new { message = "File is required." });

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
                multipart.File.FileName,
                multipart.File.ContentType ?? "application/pdf",
                multipart.File.Length);

            await using var stream = multipart.File.OpenReadStream();

            var created = await _uploadDocumentVersionService.UploadAsync(
                id,
                userId,
                stream,
                request,
                cancellationToken);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    private static Guid GetRequiredGuid(MultipartFormDataRequest multipart, string fieldName)
    {
        var value = multipart.GetValue(fieldName);
        if (!Guid.TryParse(value, out var parsed))
            throw new FormatException($"{fieldName} is invalid.");

        return parsed;
    }

    private static DateTime? GetOptionalDateTime(MultipartFormDataRequest multipart, string fieldName)
    {
        var value = multipart.GetValue(fieldName);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!DateTime.TryParse(value, out var parsed))
            throw new FormatException($"{fieldName} is invalid.");

        return parsed;
    }

    private static bool GetBoolean(MultipartFormDataRequest multipart, string fieldName)
    {
        var value = multipart.GetValue(fieldName);
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!bool.TryParse(value, out var parsed))
            throw new FormatException($"{fieldName} is invalid.");

        return parsed;
    }

    private static List<Guid>? GetGuidList(MultipartFormDataRequest multipart, string fieldName)
    {
        var values = multipart.GetValues(fieldName);
        if (values.Count == 0)
            return null;

        var ids = new List<Guid>(values.Count);
        foreach (var value in values)
        {
            if (!Guid.TryParse(value, out var parsed))
                throw new FormatException($"{fieldName} contains an invalid identifier.");

            ids.Add(parsed);
        }

        return ids;
    }
}
