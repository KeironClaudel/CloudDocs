using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Documents.SendDocumentToClient;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for send document to client service.
/// </summary>
public class SendDocumentToClientServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IDocumentVersionRepository> _documentVersionRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IDemoPolicyService> _demoPolicyServiceMock = new();
    private readonly Mock<ISentEmailLogRepository> _sentEmailLogRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private SendDocumentToClientService CreateService()
    {
        return new SendDocumentToClientService(
            _documentRepositoryMock.Object,
            _documentVersionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _emailServiceMock.Object,
            _auditServiceMock.Object,
            _demoPolicyServiceMock.Object,
            _sentEmailLogRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that execute async should throw when document does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new SendDocumentToClientRequest(null, null);

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(documentId, userId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Document not found.");
    }

    /// <summary>
    /// Verifies that execute async should throw when client email is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenClientEmailIsMissing()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new SendDocumentToClientRequest(null, null);

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "contract",
            FileExtension = ".pdf",
            Client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                IsActive = true,
                Email = null
            }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var service = CreateService();
        var act = async () => await service.ExecuteAsync(documentId, userId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Client does not have an email configured.");
    }

    /// <summary>
    /// Verifies that execute async should send the email and persist the log when request is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldSendEmailAndPersistLog_WhenRequestIsValid()
    {
        var documentId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var request = new SendDocumentToClientRequest("  ", "  ");

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "contract",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            StoragePath = "clients/contoso/contracts/file.pdf",
            Client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                Email = "client@contoso.com",
                IsActive = true
            }
        };

        var currentUser = new User
        {
            Id = currentUserId,
            FullName = "Admin User",
            Email = "admin@clouddocs.com",
            IsActive = true
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(document.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        _sentEmailLogRepositoryMock
            .Setup(x => x.CountByUserAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();
        await service.ExecuteAsync(documentId, currentUserId, request);

        _demoPolicyServiceMock.Verify(
            x => x.ValidateSendEmailAsync(currentUser, 1, It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                "client@contoso.com",
                "Document: contract.pdf",
                "Dear client,\n\nPlease find attached the document 'contract.pdf'.",
                "contract.pdf",
                fileStream,
                "application/pdf",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditServiceMock.Verify(
            x => x.LogAsync(
                currentUserId,
                "SendToClient",
                "Documents",
                "Document",
                documentId.ToString(),
                "Document sent to client: client@contoso.com",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _sentEmailLogRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<SentEmailLog>(log =>
                    log.DocumentId == documentId &&
                    log.SentByUserId == currentUserId &&
                    log.RecipientEmail == "client@contoso.com" &&
                    log.Subject == "Document: contract.pdf"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that execute async should send the selected version when request includes version id.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_ShouldSendSelectedVersion_WhenVersionIdIsProvided()
    {
        var documentId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var request = new SendDocumentToClientRequest("Versioned document", "Attached.", versionId);

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "contract",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            StoragePath = "clients/contoso/contracts/current.pdf",
            Client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                Email = "client@contoso.com",
                IsActive = true
            }
        };

        var selectedVersion = new DocumentVersion
        {
            Id = versionId,
            DocumentId = documentId,
            VersionNumber = 3,
            StoragePath = "clients/contoso/contracts/versions/v3.pdf"
        };

        var currentUser = new User
        {
            Id = currentUserId,
            FullName = "Admin User",
            Email = "admin@clouddocs.com",
            IsActive = true
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _documentVersionRepositoryMock
            .Setup(x => x.GetByIdAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(selectedVersion);

        _fileStorageServiceMock
            .Setup(x => x.GetFileAsync(selectedVersion.StoragePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUser);

        _sentEmailLogRepositoryMock
            .Setup(x => x.CountByUserAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = CreateService();
        await service.ExecuteAsync(documentId, currentUserId, request);

        _fileStorageServiceMock.Verify(
            x => x.GetFileAsync(selectedVersion.StoragePath, It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                "client@contoso.com",
                "Versioned document",
                "Attached.",
                "contract_v3.pdf",
                fileStream,
                "application/pdf",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditServiceMock.Verify(
            x => x.LogAsync(
                currentUserId,
                "SendToClient",
                "Documents",
                "DocumentVersion",
                versionId.ToString(),
                "Document version sent to client: client@contoso.com",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
