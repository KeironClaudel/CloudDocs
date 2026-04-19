using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.DeactivateDocument;
using CloudDocs.Application.Features.Documents.GetDocumentById;
using CloudDocs.Application.Features.Documents.GetDocumentFile;
using CloudDocs.Application.Features.Documents.ReactivateDocument;
using CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for uncovered document query and state services.
/// </summary>
public class DocumentQueryAndStateServicesTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IDocumentAccessService> _documentAccessServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IAuditLogQueue> _auditLogQueueMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IDocumentVersionRepository> _documentVersionRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAccessIsDenied()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "User", Email = "user@test.com", IsActive = true };
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _documentAccessServiceMock.Setup(x => x.CanAccessDocument(user, document)).Returns(false);

        var result = await new GetDocumentByIdService(_documentRepositoryMock.Object, _documentAccessServiceMock.Object)
            .GetByIdAsync(user, documentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapDocument_WhenAccessIsAllowed()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "User", Email = "user@test.com", IsActive = true };
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _documentAccessServiceMock.Setup(x => x.CanAccessDocument(user, document)).Returns(true);

        var result = await new GetDocumentByIdService(_documentRepositoryMock.Object, _documentAccessServiceMock.Object)
            .GetByIdAsync(user, documentId);

        result.Should().NotBeNull();
        result!.ClientName.Should().Be("Contoso");
        result.VisibleDepartments.Should().ContainSingle();
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnNull_WhenVersionDoesNotBelongToDocument()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "User", Email = "user@test.com", IsActive = true };
        var documentId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var document = CreateDocument(documentId);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _documentAccessServiceMock.Setup(x => x.CanAccessDocument(user, document)).Returns(true);
        _documentVersionRepositoryMock.Setup(x => x.GetByIdAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentVersion { Id = versionId, DocumentId = Guid.NewGuid(), VersionNumber = 2 });

        var service = new GetDocumentFileService(
            _documentRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _auditLogQueueMock.Object,
            _documentAccessServiceMock.Object,
            _documentVersionRepositoryMock.Object);

        var result = await service.GetFileAsync(user, documentId, "Download", user.Id, versionId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnCurrentDocumentFile_WhenRequestIsValid()
    {
        var user = new User { Id = Guid.NewGuid(), FullName = "User", Email = "user@test.com", IsActive = true };
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId);
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _documentAccessServiceMock.Setup(x => x.CanAccessDocument(user, document)).Returns(true);
        _fileStorageServiceMock.Setup(x => x.GetFileAsync(document.StoragePath, It.IsAny<CancellationToken>())).ReturnsAsync(stream);

        var service = new GetDocumentFileService(
            _documentRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _auditLogQueueMock.Object,
            _documentAccessServiceMock.Object,
            _documentVersionRepositoryMock.Object);

        var result = await service.GetFileAsync(user, documentId, "Download", user.Id);

        result.Should().NotBeNull();
        result!.Value.FileName.Should().Be("contract.pdf");
        result.Value.ContentType.Should().Be("application/pdf");

        _auditLogQueueMock.Verify(
            x => x.QueueAsync(
                It.Is<AuditLogRequest>(request =>
                    request.UserId == user.Id &&
                    request.Action == "Download" &&
                    request.Module == "Documents" &&
                    request.EntityName == "Document" &&
                    request.EntityId == documentId.ToString() &&
                    request.Details == "Download document: contract.pdf"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((Document?)null);

        var result = await new DeactivateDocumentService(_documentRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .DeactivateAsync(documentId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateDocument_WhenDocumentExists()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId);
        document.IsActive = false;
        document.DeletedAt = DateTime.UtcNow;

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var result = await new ReactivateDocumentService(_documentRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .ReactivateAsync(documentId);

        result.Should().BeTrue();
        document.IsActive.Should().BeTrue();
        document.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetByDocumentIdAsync_ShouldReturnEmptyList_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((Document?)null);

        var result = await new GetDocumentVersionsService(_documentRepositoryMock.Object, _documentVersionRepositoryMock.Object)
            .GetByDocumentIdAsync(documentId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDocumentIdAsync_ShouldMapVersions_WhenVersionsExist()
    {
        var documentId = Guid.NewGuid();
        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateDocument(documentId));
        _documentVersionRepositoryMock.Setup(x => x.GetByDocumentIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<DocumentVersion>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                VersionNumber = 2,
                StoredFileName = "v2.pdf",
                StoragePath = "stored/v2.pdf",
                UploadedByUserId = Guid.NewGuid(),
                UploadedByUser = new User { FullName = "Keiron" },
                CreatedAt = DateTime.UtcNow
            }
        });

        var result = await new GetDocumentVersionsService(_documentRepositoryMock.Object, _documentVersionRepositoryMock.Object)
            .GetByDocumentIdAsync(documentId);

        result.Should().ContainSingle();
        result[0].VersionNumber.Should().Be(2);
        result[0].UploadedByUserName.Should().Be("Keiron");
    }

    private static Document CreateDocument(Guid documentId)
    {
        var department = new Department { Id = Guid.NewGuid(), Name = "Finance", IsActive = true };
        return new Document
        {
            Id = documentId,
            OriginalFileName = "contract",
            StoredFileName = "stored.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "stored/path.pdf",
            CategoryId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Contracts", IsActive = true },
            UploadedByUserId = Guid.NewGuid(),
            UploadedByUser = new User { Id = Guid.NewGuid(), FullName = "Uploader", Email = "u@test.com", IsActive = true },
            Month = 1,
            Year = 2026,
            DocumentTypeId = Guid.NewGuid(),
            DocumentType = new DocumentTypeEntity { Id = Guid.NewGuid(), Name = "Contract", IsActive = true },
            ExpirationDate = new DateTime(2026, 12, 31),
            ExpirationDatePendingDefinition = false,
            AccessLevelId = Guid.NewGuid(),
            AccessLevel = new AccessLevelEntity { Id = Guid.NewGuid(), Name = "Internal Public", Code = "INTERNAL_PUBLIC", IsActive = true },
            ClientId = Guid.NewGuid(),
            Client = new Client { Id = Guid.NewGuid(), Name = "Contoso", IsActive = true },
            DocumentDepartments = new List<DocumentDepartment>
            {
                new() { DocumentId = documentId, DepartmentId = department.Id, Department = department }
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
