using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

public class UploadDocumentService : IUploadDocumentService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly IDocumentVersionRepository _documentVersionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<UploadDocumentService> _logger;

    public UploadDocumentService(
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IOptions<FileStorageSettings> fileStorageOptions,
        IAuditService auditService,
        IDocumentVersionRepository documentVersionRepository,
        IUnitOfWork unitOfWork,
        ILogger<UploadDocumentService> logger)
    {
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _fileStorageSettings = fileStorageOptions.Value;
        _auditService = auditService;
        _documentVersionRepository = documentVersionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

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

        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new NotFoundException("Current user not found or inactive.");

        var requiresExpiration =
            request.DocumentType == DocumentType.Contract ||
            request.DocumentType == DocumentType.Permit ||
            request.DocumentType == DocumentType.Policy ||
            request.DocumentType == DocumentType.LegalDocument;

        if (requiresExpiration && !request.ExpirationDate.HasValue && !request.ExpirationDatePendingDefinition)
            throw new BadRequestException("Expiration date or pending definition is required for this document type.");

        var sanitizedOriginalName = Path.GetFileNameWithoutExtension(request.OriginalFileName).Trim();

        if (string.IsNullOrWhiteSpace(sanitizedOriginalName))
            throw new BadRequestException("Original file name is invalid.");

        var uniqueFileName = $"{Guid.NewGuid()}.pdf";

        var storedPath = await _fileStorageService.SaveFileAsync(fileStream, uniqueFileName, cancellationToken);

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
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year,
            DocumentType = request.DocumentType,
            ExpirationDate = request.ExpirationDate,
            ExpirationDatePendingDefinition = request.ExpirationDatePendingDefinition,
            AccessLevel = request.AccessLevel,
            Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        var initialVersion = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = 1,
            StoredFileName = document.StoredFileName,
            StoragePath = document.StoragePath,
            UploadedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _documentVersionRepository.AddAsync(initialVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
                                "Document {DocumentId} uploaded by user {UserId} with original name {FileName}.",
                                document.Id,
                                user.Id,
                                document.OriginalFileName);

        await _auditService.LogAsync(
                                    user.Id,
                                    "Upload",
                                    "Documents",
                                    "Document",
                                    document.Id.ToString(),
                                    $"Document uploaded: {document.OriginalFileName}{document.FileExtension}",
                                    null,
                                    cancellationToken);

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
            document.DocumentType,
            document.ExpirationDate,
            document.ExpirationDatePendingDefinition,
            document.AccessLevel,
            document.Department,
            document.IsActive,
            document.CreatedAt);
    }
}