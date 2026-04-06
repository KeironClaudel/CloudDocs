using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.Common;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.DocumentTypes;

public class CreateDocumentTypeServiceTests
{
    private readonly Mock<IDocumentTypeRepository> _documentTypeRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateAsync_ShouldCreateDocumentType_WhenRequestIsValid()
    {
        var request = new CreateDocumentTypeRequest("Contract", "Contract files");

        _documentTypeRepositoryMock
            .Setup(x => x.NameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new CreateDocumentTypeService(
            _documentTypeRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var result = await service.CreateAsync(request);

        result.Name.Should().Be("Contract");

        _documentTypeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentTypeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameAlreadyExists()
    {
        var request = new CreateDocumentTypeRequest("Contract", "Contract files");

        _documentTypeRepositoryMock
            .Setup(x => x.NameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CreateDocumentTypeService(
            _documentTypeRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var act = async () => await service.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Document type name is already in use.");
    }
}
