using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.DocumentTypes.DeactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypeById;
using CloudDocs.Application.Features.DocumentTypes.GetDocumentTypes;
using CloudDocs.Application.Features.DocumentTypes.ReactivateDocumentType;
using CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.DocumentTypes;

/// <summary>
/// Contains tests for uncovered document type services.
/// </summary>
public class DocumentTypeServicesTests
{
    private readonly Mock<IDocumentTypeRepository> _repositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenDocumentTypeDoesNotExist()
    {
        var id = Guid.NewGuid();
        var request = new UpdateDocumentTypeRequest("Contract", null);
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((DocumentTypeEntity?)null);

        var result = await new UpdateDocumentTypeService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .UpdateAsync(id, request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequest_WhenNameBelongsToAnotherEntity()
    {
        var id = Guid.NewGuid();
        var entity = new DocumentTypeEntity { Id = id, Name = "Invoice", IsActive = true };
        var request = new UpdateDocumentTypeRequest("Contract", null);
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(x => x.NameExistsAsync(request.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await new UpdateDocumentTypeService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .UpdateAsync(id, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Document type name is already in use.");
    }

    [Fact]
    public async Task GetAllAsync_ShouldMapDocumentTypes()
    {
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DocumentTypeEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Contract", IsActive = true, CreatedAt = DateTime.UtcNow }
        });

        var result = await new GetDocumentTypesService(_repositoryMock.Object).GetAllAsync();

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Contract");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentTypeDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((DocumentTypeEntity?)null);

        var result = await new GetDocumentTypeByIdService(_repositoryMock.Object).GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateAsync_ShouldReturnFalse_WhenDocumentTypeDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((DocumentTypeEntity?)null);

        var result = await new DeactivateDocumentTypeService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .DeactivateAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateDocumentType_WhenEntityExists()
    {
        var id = Guid.NewGuid();
        var entity = new DocumentTypeEntity { Id = id, Name = "Contract", IsActive = false, DeletedAt = DateTime.UtcNow };
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await new ReactivateDocumentTypeService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .ReactivateAsync(id);

        result.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.DeletedAt.Should().BeNull();
    }
}
