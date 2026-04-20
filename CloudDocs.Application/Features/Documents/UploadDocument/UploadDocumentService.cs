using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Helpers;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

/// <summary>
/// Provides operations for upload document.
/// </summary>
public class UploadDocumentService : IUploadDocumentService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly IDocumentVersionRepository _documentVersionRepository;
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IAccessLevelRepository _accessLevelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditLogQueue _auditLogQueue;
    private readonly IDemoPolicyService _demoPolicyService;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<UploadDocumentService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadDocumentService"/> class.
    /// </summary>
    /// <param name="categoryRepository">The category repository.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    /// <param name="fileStorageOptions">The file storage options.</param>
    /// <param name="auditLogQueue">The audit log queue.</param>
    /// <param name="documentVersionRepository">The document version repository.</param>
    /// <param name="documentTypeRepository">The document type repository.</param>
    /// <param name="accessLevelRepository">The document access level repository.</param>
    /// <param name="departmentRepository">The document department repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="demoPolicyService">The demo policy service.</param>
    /// <param name="clientRepository">The client repository.</param>
    /// <param name="logger">The logger.</param>
    public UploadDocumentService(
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IOptions<FileStorageSettings> fileStorageOptions,
        IAuditLogQueue auditLogQueue,
        IDocumentVersionRepository documentVersionRepository,
        IDocumentTypeRepository documentTypeRepository,
        IAccessLevelRepository accessLevelRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IClientRepository clientRepository,
        IDemoPolicyService demoPolicyService,
        ILogger<UploadDocumentService> logger)
    {
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _fileStorageSettings = fileStorageOptions.Value;
        _auditLogQueue = auditLogQueue;
        _documentVersionRepository = documentVersionRepository;
        _documentTypeRepository = documentTypeRepository;
        _accessLevelRepository = accessLevelRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _clientRepository = clientRepository;
        _demoPolicyService = demoPolicyService;
        _logger = logger;
    }

    /// <summary>
    /// Uploads.
    /// </summary>
    /// <param name="currentUserId">The current user id identifier.</param>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document response.</returns>
    public async Task<DocumentResponse> UploadAsync(
        Guid currentUserId,
        Stream fileStream,
        UploadDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
            throw new BadRequestException("File stream is required.");

        if (request.FileSize <= 0)
            throw new BadRequestException("File is required.");

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            throw new BadRequestException("Original file name is required.");

        var extension = Path.GetExtension(request.OriginalFileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Only .pdf files are allowed.");

        if (!string.IsNullOrWhiteSpace(request.ContentType) &&
            !string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Only PDF files are allowed.");
        }

        if (request.FileSize > _fileStorageSettings.MaxFileSizeInBytes)
            throw new BadRequestException("File exceeds maximum allowed size.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
            throw new NotFoundException("Category not found or inactive.");

        var documentType = await _documentTypeRepository.GetByIdAsync(request.DocumentTypeId, cancellationToken);
        if (documentType is null || !documentType.IsActive)
            throw new NotFoundException("Document type not found or inactive.");

        // If the document type requires an expiration date or pending definition, validate it
        //if (documentType.RequiresExpiration && !request.ExpirationDate.HasValue && !request.ExpirationDatePendingDefinition)
        //    throw new BadRequestException("Expiration date or pending definition is required for this document type.");

        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new NotFoundException("Current user not found or inactive.");

        var accessLevel = await _accessLevelRepository.GetByIdAsync(request.AccessLevelId, cancellationToken);
        if (accessLevel is null || !accessLevel.IsActive)
            throw new NotFoundException("Access level not found or inactive.");

        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);

        if (client is null || !client.IsActive)
            throw new BadRequestException("Client not found or inactive.");

        var selectedDepartments = new List<Department>();

        if (string.Equals(accessLevel.Code, "DEPARTMENT_ONLY", StringComparison.OrdinalIgnoreCase))
        {
            if (request.DepartmentIds is null || request.DepartmentIds.Count == 0)
                throw new BadRequestException("At least one department must be selected for department-only access.");

            selectedDepartments = await _departmentRepository.GetByIdsAsync(request.DepartmentIds, cancellationToken);

            if (selectedDepartments.Count != request.DepartmentIds.Distinct().Count())
                throw new BadRequestException("One or more selected departments are invalid or inactive.");
        }

        var sanitizedOriginalName = Path.GetFileNameWithoutExtension(request.OriginalFileName).Trim();

        if (string.IsNullOrWhiteSpace(sanitizedOriginalName))
            throw new BadRequestException("Original file name is invalid.");

        var uniqueFileName = $"{Guid.NewGuid()}.pdf";
        var uploadedAt = DateTime.UtcNow;

        var storagePath = StoragePathBuilder.BuildClientCategoryPath(
            client.Name,
            category.Name,
            documentType.Name,
            uniqueFileName,
            uploadedAt);

        var currentDocumentCount = await _documentRepository.CountByUserAsync(user.Id, cancellationToken);

        await _demoPolicyService.ValidateUploadAsync(
            user,
            request.ContentType,
            request.FileSize,
            currentDocumentCount,
            cancellationToken);

        var storedPath = await _fileStorageService.SaveFileAsync(fileStream, storagePath, cancellationToken);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            OriginalFileName = sanitizedOriginalName,
            StoredFileName = uniqueFileName,
            FileExtension = extension.ToLower(),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/pdf" : request.ContentType,
            FileSize = request.FileSize,
            StoragePath = storedPath,
            CategoryId = category.Id,
            UploadedByUserId = user.Id,
            Month = uploadedAt.Month,
            Year = uploadedAt.Year,
            DocumentTypeId = documentType.Id,
            DocumentType = documentType,
            ExpirationDate = DateTimeHelper.EnsureUtc(request.ExpirationDate),
            ExpirationDatePendingDefinition = request.ExpirationDatePendingDefinition,
            AccessLevelId = accessLevel.Id,
            AccessLevel = accessLevel,
            ClientId = client.Id,
            Client = client,
            IsActive = true,
            CreatedAt = uploadedAt
        };

        if (selectedDepartments.Count > 0)
        {
            document.DocumentDepartments = selectedDepartments
                .Select(d => new DocumentDepartment
                {
                    DocumentId = document.Id,
                    DepartmentId = d.Id
                })
                .ToList();
        }

        await _documentRepository.AddAsync(document, cancellationToken);

        var initialVersion = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = 1,
            StoredFileName = document.StoredFileName,
            StoragePath = document.StoragePath,
            UploadedByUserId = user.Id,
            CreatedAt = uploadedAt
        };

        await _documentVersionRepository.AddAsync(initialVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
                                "Document {DocumentId} uploaded by user {UserId} with original name {FileName}.",
                                document.Id,
                                user.Id,
                                document.OriginalFileName);

        await _auditLogQueue.QueueAsync(
            new AuditLogRequest(
                user.Id,
                "Upload",
                "Documents",
                "Document",
                document.Id.ToString(),
                $"Document uploaded: {document.OriginalFileName}{document.FileExtension}"),
            cancellationToken);

        var visibleDepartments = selectedDepartments.Count > 0
            ? selectedDepartments.Select(d => new DocumentDepartmentResponse(d.Id, d.Name)).ToList()
            : document.DocumentDepartments
                .Select(dd => new DocumentDepartmentResponse(dd.DepartmentId, dd.Department?.Name ?? string.Empty))
                .ToList();

        return new DocumentResponse(
            document.Id,
            document.OriginalFileName,
            document.StoredFileName,
            document.FileExtension,
            document.ContentType,
            document.FileSize,
            document.CategoryId,
            category.Name,
            document.UploadedByUserId,
            user.FullName,
            document.Month,
            document.Year,
            documentType.Id,
            documentType.Name,
            document.ExpirationDate,
            document.ExpirationDatePendingDefinition,
            accessLevel.Id,
            accessLevel.Name,
            accessLevel.Code,
            client.Id,
            client.Name,
            visibleDepartments,
            document.IsActive,
            document.CreatedAt);
    }
}
