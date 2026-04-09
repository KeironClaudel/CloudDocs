using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Documents.RenameDocument;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for rename document service.
/// </summary>
public class RenameDocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RenameDocumentService CreateService()
    {
        return new RenameDocumentService(
            _documentRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that rename document should throw exception when new name is empty.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RenameAsync_ShouldThrowBadRequest_WhenNewNameIsEmpty()
    {
        var documentId = Guid.NewGuid();
        var request = new RenameDocumentRequest("");

        var service = CreateService();
        var act = async () => await service.RenameAsync(documentId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("New name is required.");
    }

    /// <summary>
    /// Verifies that rename document should throw exception when new name is null.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RenameAsync_ShouldThrowBadRequest_WhenNewNameIsNull()
    {
        var documentId = Guid.NewGuid();
        var request = new RenameDocumentRequest(null!);

        var service = CreateService();
        var act = async () => await service.RenameAsync(documentId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("New name is required.");
    }

    /// <summary>
    /// Verifies that rename document should throw exception when new name is whitespace.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RenameAsync_ShouldThrowBadRequest_WhenNewNameIsWhitespace()
    {
        var documentId = Guid.NewGuid();
        var request = new RenameDocumentRequest("   ");

        var service = CreateService();
        var act = async () => await service.RenameAsync(documentId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("New name is required.");
    }

    /// <summary>
    /// Verifies that rename document should return false when document does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RenameAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var request = new RenameDocumentRequest("New Name.pdf");

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = CreateService();
        var result = await service.RenameAsync(documentId, request);

        result.Should().BeFalse();
        _documentRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that rename document should rename document successfully.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RenameAsync_ShouldRenameDocument_WhenDocumentExistsAndNameIsValid()
    {
        var documentId = Guid.NewGuid();
        var newName = "Updated Document Name.pdf";
        var request = new RenameDocumentRequest(newName);
        var oldUpdatedAt = DateTime.UtcNow.AddHours(-1);

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "Original Name.pdf",
            StoredFileName = "stored-123.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            CategoryId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            DocumentTypeId = Guid.NewGuid(),
            AccessLevelId = Guid.NewGuid(),
            IsActive = true,
            UpdatedAt = oldUpdatedAt
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var service = CreateService();
        var result = await service.RenameAsync(documentId, request);

        result.Should().BeTrue();
        document.OriginalFileName.Should().Be(newName);
        document.UpdatedAt.Should().BeAfter(oldUpdatedAt);

        _documentRepositoryMock.Verify(
            x => x.UpdateAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                null,
                "Rename",
                "Documents",
                "Document",
                documentId.ToString(),
                $"Document renamed to: {newName}",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that rename document should trim whitespace from new name.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RenameAsync_ShouldTrimWhitespace_WhenRenamingDocument()
    {
        var documentId = Guid.NewGuid();
        var newNameWithWhitespace = "  Document Name with Spaces.pdf  ";
        var expectedName = "Document Name with Spaces.pdf";
        var request = new RenameDocumentRequest(newNameWithWhitespace);

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "Original.pdf",
            StoredFileName = "stored-456.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSize = 2048,
            CategoryId = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            DocumentTypeId = Guid.NewGuid(),
            AccessLevelId = Guid.NewGuid(),
            IsActive = true
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var service = CreateService();
        await service.RenameAsync(documentId, request);

        document.OriginalFileName.Should().Be(expectedName);
    }
}
