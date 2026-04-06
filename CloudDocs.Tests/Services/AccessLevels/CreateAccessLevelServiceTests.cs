using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;
using CloudDocs.Application.Features.AccessLevels.Common;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.AccessLevels;

public class CreateAccessLevelServiceTests
{
    private readonly Mock<IAccessLevelRepository> _accessLevelRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateAsync_ShouldCreateAccessLevel_WhenRequestIsValid()
    {
        var request = new CreateAccessLevelRequest("INTERNAL_PUBLIC", "Internal Public", "Public to authenticated users");

        _accessLevelRepositoryMock
            .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _accessLevelRepositoryMock
            .Setup(x => x.NameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new CreateAccessLevelService(
            _accessLevelRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var result = await service.CreateAsync(request);

        result.Code.Should().Be("INTERNAL_PUBLIC");

        _accessLevelRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AccessLevelEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCodeExists()
    {
        var request = new CreateAccessLevelRequest("INTERNAL_PUBLIC", "Internal Public", "Public to authenticated users");

        _accessLevelRepositoryMock
            .Setup(x => x.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CreateAccessLevelService(
            _accessLevelRepositoryMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var act = async () => await service.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Access level code is already in use.");
    }
}
