using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for upload document version service.
/// </summary>
public class UploadDocumentVersionServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IDocumentVersionRepository> _documentVersionRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly FileStorageSettings _fileStorageSettings = new()
    {
        RootPath = "Storage/Documents",
        MaxFileSizeInBytes = 5_000_000
    };

    private UploadDocumentVersionService CreateService()
    {
        return new UploadDocumentVersionService(
            _documentRepositoryMock.Object,
            _documentVersionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _auditServiceMock.Object,
            Options.Create(_fileStorageSettings),
            _unitOfWorkMock.Object,
            NullLogger<UploadDocumentVersionService>.Instance);
    }

    /// <summary>
    /// Verifies that upload async should throw when document does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowInvalidOperation_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UploadDocumentVersionRequest("version.pdf", "application/pdf", 100);

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();
        var act = async () => await service.UploadAsync(documentId, userId, stream, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found or inactive.");
    }

    /// <summary>
    /// Verifies that upload async should throw when current user does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldThrowInvalidOperation_WhenCurrentUserDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UploadDocumentVersionRequest("version.pdf", "application/pdf", 100);

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document
            {
                Id = documentId,
                OriginalFileName = "contract",
                IsActive = true
            });

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();
        var act = async () => await service.UploadAsync(documentId, userId, stream, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Current user not found or inactive.");
    }

    /// <summary>
    /// Verifies that upload async should create a new document version and update document metadata when request is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_ShouldCreateVersionAndUpdateDocument_WhenRequestIsValid()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UploadDocumentVersionRequest("new-version.pdf", "application/pdf", 1200);

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "contract",
            StoredFileName = "old.pdf",
            StoragePath = "old/path.pdf",
            ContentType = "application/pdf",
            FileExtension = ".pdf",
            FileSize = 100,
            IsActive = true
        };

        var user = new User
        {
            Id = userId,
            FullName = "Keiron",
            Email = "keiron@test.com",
            IsActive = true
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _documentVersionRepositoryMock
            .Setup(x => x.GetNextVersionNumberAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/version-2.pdf");

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var service = CreateService();
        var result = await service.UploadAsync(documentId, userId, stream, request);

        result.DocumentId.Should().Be(documentId);
        result.VersionNumber.Should().Be(2);
        result.StoragePath.Should().Be("stored/version-2.pdf");
        result.UploadedByUserId.Should().Be(userId);
        result.UploadedByUserName.Should().Be("Keiron");

        document.StoragePath.Should().Be("stored/version-2.pdf");
        document.ContentType.Should().Be("application/pdf");
        document.FileSize.Should().Be(1200);
        document.FileExtension.Should().Be(".pdf");
        document.UploadedByUserId.Should().Be(userId);

        _documentVersionRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<DocumentVersion>(v =>
                    v.DocumentId == documentId &&
                    v.VersionNumber == 2 &&
                    v.StoragePath == "stored/version-2.pdf" &&
                    v.UploadedByUserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _documentRepositoryMock.Verify(
            x => x.UpdateAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _auditServiceMock.Verify(
            x => x.LogAsync(
                userId,
                "UploadVersion",
                "Documents",
                "Document",
                documentId.ToString(),
                "Uploaded version 2 for document: contract.pdf",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
