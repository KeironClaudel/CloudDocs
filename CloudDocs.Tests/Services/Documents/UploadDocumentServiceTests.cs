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

public class UploadDocumentServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IDocumentVersionRepository> _documentVersionRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
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
        _unitOfWorkMock.Object,
        NullLogger<UploadDocumentService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenFileSizeIsZero()
    {
        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            0,
            Guid.NewGuid(),
            DocumentType.General,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
            null);

        using var stream = new MemoryStream();
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("File is required.");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenOriginalFileNameIsEmpty()
    {
        var request = new UploadDocumentRequest(
            "   ",
            "application/pdf",
            100,
            Guid.NewGuid(),
            DocumentType.General,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
            null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Original file name is required.");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenExtensionIsNotPdf()
    {
        var request = new UploadDocumentRequest(
            "test.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            100,
            Guid.NewGuid(),
            DocumentType.General,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
            null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Only .pdf files are allowed.");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenFileExceedsMaxSize()
    {
        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            _fileStorageSettings.MaxFileSizeInBytes + 1,
            Guid.NewGuid(),
            DocumentType.General,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
            null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(Guid.NewGuid(), stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("File exceeds maximum allowed size.");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowNotFound_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            100,
            categoryId,
            DocumentType.General,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
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

    [Fact]
    public async Task UploadAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "test.pdf",
            "application/pdf",
            100,
            categoryId,
            DocumentType.General,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
            null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category
            {
                Id = categoryId,
                Name = "Contracts",
                IsActive = true
            });

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(userId, stream, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Current user not found or inactive.");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowBadRequest_WhenExpirationIsRequiredButMissing()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "contract.pdf",
            "application/pdf",
            100,
            categoryId,
            DocumentType.Contract,
            null,
            false,
            DocumentAccessLevel.InternalPublic,
            null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category
            {
                Id = categoryId,
                Name = "Contracts",
                IsActive = true
            });

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

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();

        var act = async () => await service.UploadAsync(userId, stream, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Expiration date or pending definition is required for this document type.");
    }

    [Fact]
    public async Task UploadAsync_ShouldCreateDocumentAndInitialVersion_WhenRequestIsValid()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new UploadDocumentRequest(
            "contract.pdf",
            "application/pdf",
            100,
            categoryId,
            DocumentType.Contract,
            new DateTime(2026, 12, 31),
            false,
            DocumentAccessLevel.InternalPublic,
            "Finance");

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

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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
        result.DocumentType.Should().Be(DocumentType.Contract);
        result.ExpirationDate.Should().Be(new DateTime(2026, 12, 31));
        result.AccessLevel.Should().Be(DocumentAccessLevel.InternalPublic);
        result.Department.Should().Be("Finance");
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