using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.UploadDocument;
using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for upload document service.
/// </summary>
public class UploadDocumentServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IDocumentVersionRepository> _documentVersionRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IDocumentTypeRepository> _documentTypeRepositoryMock = new();
    private readonly Mock<IAccessLevelRepository> _accessLevelRepositoryMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IDemoPolicyService> _demoPolicyServiceMock = new();
    private readonly Mock<IClientRepository> _clientRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly FileStorageSettings _fileStorageSettings = new()
    {
        RootPath = "Storage/Documents",
        MaxFileSizeInBytes = 5_000_000
    };

    private UploadDocumentService CreateService()
    {
        var options = Options.Create(_fileStorageSettings);

        return new UploadDocumentService(
        _categoryRepositoryMock.Object,
        _userRepositoryMock.Object,
        _documentRepositoryMock.Object,
        _fileStorageServiceMock.Object,
        options,
        _auditServiceMock.Object,
        _documentVersionRepositoryMock.Object,
        _documentTypeRepositoryMock.Object,
        _accessLevelRepositoryMock.Object,
        _departmentRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _clientRepositoryMock.Object,
        _demoPolicyServiceMock.Object,
        NullLogger<UploadDocumentService>.Instance);
    }

    /// <summary>
    /// Verifies that upload async should throw bad request when file size is zero.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenFileSizeIsZero()
    {
        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            0,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        using var stream = new MemoryStream();
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("File is required.");
    }

    /// <summary>
    /// Verifies that upload async should throw bad request when original file name is empty.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenOriginalFileNameIsEmpty()
    {
        var request = new UploadDocumentRequest(
            "   ",
            "application/pdf",
            100,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Original file name is required.");
    }

    /// <summary>
    /// Verifies that upload async should throw bad request when extension is not pdf.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenExtensionIsNotPdf()
    {
        var request = new UploadDocumentRequest(
            "test.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            100,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Only .pdf files are allowed.");
    }

    /// <summary>
    /// Verifies that upload async should throw bad request when file exceeds max size.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenFileExceedsMaxSize()
    {
        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            _fileStorageSettings.MaxFileSizeInBytes + 1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("File exceeds maximum allowed size.");
    }

    /// <summary>
    /// Verifies that upload async should throw not found when category does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowNotFound_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            100,
            categoryId,
            Guid.NewGuid(),
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category not found or inactive.");
    }

    /// <summary>
    /// Verifies that upload async should throw not found when user does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            100,
            categoryId,
            documentTypeId,
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category
            {
                Id = categoryId,
                Name = "Contracts",
                IsActive = true
            });

        // ensure document type exists so the service continues to user lookup
        _documentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(documentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CloudDocs.Domain.Entities.DocumentTypeEntity { Id = documentTypeId, Name = "General", IsActive = true });

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(userId, stream, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Current user not found or inactive.");
    }

    /// <summary>
    /// Verifies that upload async should throw bad request when expiration is required but missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenExpirationIsRequiredButMissing()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var documentTypeId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "contract.pdf",
            "application/pdf",
            100,
            categoryId,
            // use the contract document type id from db
            documentTypeId,
            null,
            false,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category
            {
                Id = categoryId,
                Name = "Contracts",
                IsActive = true
            });

        // ensure document type exists and requires expiration so the service validates expiration
        _documentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(documentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CloudDocs.Domain.Entities.DocumentTypeEntity { Id = documentTypeId, Name = "Contract", IsActive = true });

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                FullName = "Keiron",
                Email = "keiron@test.com",
                IsActive = true,
                Role = new Role { Name = "Admin" }
            });

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(request.AccessLevelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CloudDocs.Domain.Entities.AccessLevelEntity
            {
                Id = request.AccessLevelId,
                Name = "Internal Public",
                Code = "INTERNAL_PUBLIC",
                IsActive = true
            });

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(userId, stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Expiration date or pending definition is required for this document type.");
    }

    /// <summary>
    /// Verifies that upload async should create document and initial version when request is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldCreateDocumentAndInitialVersion_WhenRequestIsValid()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var accessLevelId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var request = new UploadDocumentRequest(
            "contract.pdf",
            "application/pdf",
            100,
            categoryId,
            documentTypeId,
            new DateTime(2026, 12, 31),
            false,
            accessLevelId, // Updated access level
            Guid.Empty,
            new List<Guid> { departmentId }); // Updated department list

        var category = new Category
        {
            Id = categoryId,
            Name = "Contracts",
            IsActive = true
        };

        var user = new User
        {
            Id = userId,
            FullName = "Keiron",
            Email = "keiron@test.com",
            IsActive = true,
            Role = new Role { Name = "Admin" }
        };

        var documentTypeEntity = new CloudDocs.Domain.Entities.DocumentTypeEntity
        {
            Id = documentTypeId,
            Name = "Contract",
            IsActive = true
        };

        var accessLevelEntity = new AccessLevelEntity
        {
            Id = accessLevelId,
            Name = "Department Only",
            Code = "DEPARTMENT_ONLY",
            IsActive = true
        };

        var department = new Department
        {
            Id = departmentId,
            Name = "Finance",
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _documentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(documentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentTypeEntity);

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(accessLevelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessLevelEntity);

        _departmentRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => new List<Department> { department });

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream _, string fileName, CancellationToken _) => fileName);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var result = await service.UploadAsync(userId, stream, request);

        result.OriginalFileName.Should().Be("contract");
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be("Contracts");
        result.UploadedByUserId.Should().Be(userId);
        result.UploadedByUserName.Should().Be("Keiron");
        result.DocumentTypeId.Should().Be(documentTypeId);
        result.DocumentTypeName.Should().Be("Contract");
        result.AccessLevelId.Should().Be(accessLevelId);
        result.AccessLevelName.Should().Be("Department Only");
        result.AccessLevelCode.Should().Be("DEPARTMENT_ONLY");
        result.VisibleDepartments.Should().ContainSingle()
            .Which.Name.Should().Be("Finance");
        result.ExpirationDate.Should().Be(new DateTime(2026, 12, 31));
        result.FileExtension.Should().Be(".pdf");

        _documentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _documentVersionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DocumentVersion>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}