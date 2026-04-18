using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Documents.UpdateDocumentVisibility;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for update document visibility service.
/// </summary>
public class UpdateDocumentVisibilityServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IAccessLevelRepository> _accessLevelRepositoryMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();

    private UpdateDocumentVisibilityService CreateService()
    {
        return new UpdateDocumentVisibilityService(
            _documentRepositoryMock.Object,
            _accessLevelRepositoryMock.Object,
            _departmentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _auditServiceMock.Object);
    }

    /// <summary>
    /// Verifies that update async should return false when document does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var request = new UpdateDocumentVisibilityRequest(Guid.NewGuid(), null);

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(documentId, request);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that update async should throw when access level does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenAccessLevelDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var accessLevelId = Guid.NewGuid();
        var request = new UpdateDocumentVisibilityRequest(accessLevelId, null);

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = documentId, DocumentDepartments = new List<DocumentDepartment>() });

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(accessLevelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessLevelEntity?)null);

        var service = CreateService();
        var act = async () => await service.UpdateAsync(documentId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Access level not found or inactive.");
    }

    /// <summary>
    /// Verifies that update async should throw when department-only visibility has no departments.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequest_WhenDepartmentOnlyHasNoDepartments()
    {
        var documentId = Guid.NewGuid();
        var accessLevelId = Guid.NewGuid();
        var request = new UpdateDocumentVisibilityRequest(accessLevelId, new List<Guid>());

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = documentId, DocumentDepartments = new List<DocumentDepartment>() });

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(accessLevelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessLevelEntity
            {
                Id = accessLevelId,
                Name = "Department Only",
                Code = "DEPARTMENT_ONLY",
                IsActive = true
            });

        var service = CreateService();
        var act = async () => await service.UpdateAsync(documentId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("At least one department must be selected.");
    }

    /// <summary>
    /// Verifies that update async should update access level and departments when request is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateAccessLevelAndDepartments_WhenRequestIsValid()
    {
        var documentId = Guid.NewGuid();
        var accessLevelId = Guid.NewGuid();
        var departmentId1 = Guid.NewGuid();
        var departmentId2 = Guid.NewGuid();
        var request = new UpdateDocumentVisibilityRequest(accessLevelId, new List<Guid> { departmentId1, departmentId2 });

        var document = new Document
        {
            Id = documentId,
            AccessLevelId = Guid.NewGuid(),
            DocumentDepartments = new List<DocumentDepartment>
            {
                new() { DocumentId = documentId, DepartmentId = Guid.NewGuid() }
            }
        };

        var accessLevel = new AccessLevelEntity
        {
            Id = accessLevelId,
            Name = "Department Only",
            Code = "DEPARTMENT_ONLY",
            IsActive = true
        };

        var departments = new List<Department>
        {
            new() { Id = departmentId1, Name = "Finance", IsActive = true },
            new() { Id = departmentId2, Name = "HR", IsActive = true }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _accessLevelRepositoryMock
            .Setup(x => x.GetByIdAsync(accessLevelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessLevel);

        _departmentRepositoryMock
            .Setup(x => x.GetByIdsAsync(request.DepartmentIds!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        var service = CreateService();
        var result = await service.UpdateAsync(documentId, request);

        result.Should().BeTrue();
        document.AccessLevelId.Should().Be(accessLevelId);
        document.DocumentDepartments.Should().HaveCount(2);
        document.DocumentDepartments.Select(x => x.DepartmentId)
            .Should().BeEquivalentTo(new[] { departmentId1, departmentId2 });

        _documentRepositoryMock.Verify(
            x => x.UpdateAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _auditServiceMock.Verify(
            x => x.LogAsync(
                null,
                "UpdateVisibility",
                "Documents",
                "Document",
                documentId.ToString(),
                "Document visibility updated to Department Only",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
