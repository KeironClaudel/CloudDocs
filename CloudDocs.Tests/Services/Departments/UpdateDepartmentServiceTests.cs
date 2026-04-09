using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Departments.Common;
using CloudDocs.Application.Features.Departments.UpdateDepartment;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Departments;

/// <summary>
/// Contains tests for update department service.
/// </summary>
public class UpdateDepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private UpdateDepartmentService CreateService()
    {
        return new UpdateDepartmentService(
            _departmentRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that update department should return null when department does not exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenDepartmentDoesNotExist()
    {
        var deptId = Guid.NewGuid();
        var request = new UpdateDepartmentRequest("New Department", "Description");

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(deptId, request);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that update department should throw exception when name already exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequest_WhenNameAlreadyExists()
    {
        var deptId = Guid.NewGuid();
        var newName = "Sales Department";
        var request = new UpdateDepartmentRequest(newName, "Description");

        var department = new Department
        {
            Id = deptId,
            Name = "Finance Department",
            IsActive = true
        };

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepositoryMock
            .Setup(x => x.NameExistsAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var act = async () => await service.UpdateAsync(deptId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Department name is already in use.");
    }

    /// <summary>
    /// Verifies that update department should update successfully when name is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully_WhenNameIsValid()
    {
        var deptId = Guid.NewGuid();
        var newName = "Updated Department";
        var newDescription = "Updated Description";
        var request = new UpdateDepartmentRequest(newName, newDescription);

        var department = new Department
        {
            Id = deptId,
            Name = "Original Department",
            Description = "Original Description",
            IsActive = true
        };

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepositoryMock
            .Setup(x => x.NameExistsAsync(newName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var result = await service.UpdateAsync(deptId, request);

        result.Should().NotBeNull();
        result!.Id.Should().Be(deptId);
        result.Name.Should().Be(newName);
        result.Description.Should().Be(newDescription);

        _departmentRepositoryMock.Verify(
            x => x.UpdateAsync(department, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditServiceMock.Verify(
            x => x.LogAsync(
                null,
                "Update",
                "Departments",
                "Department",
                deptId.ToString(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that update department should allow same name as current.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateAsync_ShouldAllowSameName_WhenUpdatingCurrentName()
    {
        var deptId = Guid.NewGuid();
        var sameName = "Finance Department";
        var request = new UpdateDepartmentRequest(sameName, null);

        var department = new Department
        {
            Id = deptId,
            Name = sameName,
            IsActive = true
        };

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepositoryMock
            .Setup(x => x.NameExistsAsync(sameName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.UpdateAsync(deptId, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be(sameName);
    }
}
