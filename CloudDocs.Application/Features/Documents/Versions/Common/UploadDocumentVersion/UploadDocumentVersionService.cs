using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Versions.Common;
using CloudDocs.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;

/// <summary>
/// Provides operations for upload document version.
/// </summary>
public class UploadDocumentVersionService : IUploadDocumentVersionService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentVersionRepository _documentVersionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditService _auditService;
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadDocumentVersionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadDocumentVersionService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="documentVersionRepository">The document version repository.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="fileStorageOptions">The file storage options.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public UploadDocumentVersionService(
        IDocumentRepository documentRepository,
        IDocumentVersionRepository documentVersionRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorageService,
        IAuditService auditService,
        IOptions<FileStorageSettings> fileStorageOptions,
        IUnitOfWork unitOfWork,
        ILogger<UploadDocumentVersionService> logger)
    {
        _documentRepository = documentRepository;
        _documentVersionRepository = documentVersionRepository;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
        _auditService = auditService;
        _fileStorageSettings = fileStorageOptions.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Uploads.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="currentUserId">The current user id identifier.</param>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document version response.</returns>
    public async Task<DocumentVersionResponse> UploadAsync(
        Guid documentId,
        Guid currentUserId,
        Stream fileStream,
        UploadDocumentVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
            throw new InvalidOperationException("File stream is required.");

        if (request.FileSize <= 0)
            throw new InvalidOperationException("File is required.");

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            throw new InvalidOperationException("Original file name is required.");

        var extension = Path.GetExtension(request.OriginalFileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only .pdf files are allowed.");

        if (!string.IsNullOrWhiteSpace(request.ContentType) &&
            !string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only PDF files are allowed.");
        }

        if (request.FileSize > _fileStorageSettings.MaxFileSizeInBytes)
            throw new InvalidOperationException("File exceeds maximum allowed size.");

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null || !document.IsActive)
            throw new InvalidOperationException("Document not found or inactive.");

        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Current user not found or inactive.");

        var nextVersionNumber = await _documentVersionRepository.GetNextVersionNumberAsync(documentId, cancellationToken);
        var uniqueFileName = $"{Guid.NewGuid()}.pdf";
        var storedPath = await _fileStorageService.SaveFileAsync(fileStream, uniqueFileName, cancellationToken);

        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = nextVersionNumber,
            StoredFileName = uniqueFileName,
            StoragePath = storedPath,
            UploadedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _documentVersionRepository.AddAsync(version, cancellationToken);

        document.StoredFileName = uniqueFileName;
        document.StoragePath = storedPath;
        document.FileSize = request.FileSize;
        document.ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/pdf" : request.ContentType;
        document.FileExtension = extension.ToLower();
        document.UpdatedAt = DateTime.UtcNow;
        document.UploadedByUserId = user.Id;
        document.Month = DateTime.UtcNow.Month;
        document.Year = DateTime.UtcNow.Year;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(
            user.Id,
            "UploadVersion",
            "Documents",
            "Document",
            document.Id.ToString(),
            $"Uploaded version {version.VersionNumber} for document: {document.OriginalFileName}{document.FileExtension}",
            null,
            cancellationToken);

        _logger.LogInformation(
                                "Version {VersionNumber} uploaded for document {DocumentId} by user {UserId}.",
                                version.VersionNumber,
                                document.Id,
                                user.Id);

        return new DocumentVersionResponse(
            version.Id,
            version.DocumentId,
            version.VersionNumber,
            version.StoredFileName,
            version.StoragePath,
            version.UploadedByUserId,
            user.FullName,
            version.CreatedAt);
    }
}