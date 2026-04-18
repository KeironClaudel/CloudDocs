using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Departments.DeactivateDepartment;
using CloudDocs.Application.Features.Departments.GetDepartmentById;
using CloudDocs.Application.Features.Departments.GetDepartments;
using CloudDocs.Application.Features.Departments.ReactivateDepartment;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Departments;

/// <summary>
/// Contains tests for uncovered department services.
/// </summary>
public class DepartmentServicesTests
{
    private readonly Mock<IDepartmentRepository> _repositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task GetAllAsync_ShouldMapDepartments()
    {
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Department>
        {
            new() { Id = Guid.NewGuid(), Name = "HR", IsActive = true, CreatedAt = DateTime.UtcNow }
        });

        var result = await new GetDepartmentsService(_repositoryMock.Object).GetAllAsync();

        result.Should().ContainSingle();
        result[0].Name.Should().Be("HR");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDepartmentDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Department?)null);

        var result = await new GetDepartmentByIdService(_repositoryMock.Object).GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateAsync_ShouldReturnFalse_WhenDepartmentDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Department?)null);

        var result = await new DeactivateDepartmentService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .DeactivateAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateDepartment_WhenDepartmentExists()
    {
        var id = Guid.NewGuid();
        var entity = new Department { Id = id, Name = "HR", IsActive = false, DeletedAt = DateTime.UtcNow };
        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await new ReactivateDepartmentService(_repositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object)
            .ReactivateAsync(id);

        result.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.DeletedAt.Should().BeNull();
    }
}
