using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Departments.CreateDepartment;
using CloudDocs.Application.Features.Departments.Common;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Departments;

public class CreateDepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateAsync_ShouldCreateDepartment_WhenRequestIsValid()
    {
        var request = new CreateDepartmentRequest("Finance", "Finance department");

        _departmentRepositoryMock
            .Setup(x => x.NameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new CreateDepartmentService(
            _departmentRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var result = await service.CreateAsync(request);

        result.Name.Should().Be("Finance");

        _departmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameExists()
    {
        var request = new CreateDepartmentRequest("Finance", "Finance department");

        _departmentRepositoryMock
            .Setup(x => x.NameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CreateDepartmentService(
            _departmentRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var act = async () => await service.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Department name is already in use.");
    }
}
