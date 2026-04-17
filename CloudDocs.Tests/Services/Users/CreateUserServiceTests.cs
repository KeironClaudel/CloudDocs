using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Users.CreateUser;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Users;

/// <summary>
/// Contains tests for create user service.
/// </summary>
public class CreateUserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    /// <summary>
    /// Verifies that create async should create user when request is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenRequestIsValid()
    {
        var roleId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        var request = new CreateUserRequest(
            "Keiron Test",
            "keiron@test.com",
            "User1234!",
            deptId,
            roleId);

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, Name = "User" });

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed-password");

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = deptId, Name = "Finance", IsActive = true });

        var service = new CreateUserService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _departmentRepositoryMock.Object,
            _passwordHasherMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var result = await service.CreateAsync(request);

        result.FullName.Should().Be("Keiron Test");
        result.Email.Should().Be("keiron@test.com");
        result.Department.Should().Be("Finance");
        result.Role.Should().Be("User");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that create async should throw when email already exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var request = new CreateUserRequest(
            "Keiron Test",
            "keiron@test.com",
            "User1234!",
            Guid.NewGuid(),
            Guid.NewGuid());

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new CreateUserService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _departmentRepositoryMock.Object,
            _passwordHasherMock.Object,
            _auditServiceMock.Object,
            _unitOfWorkMock.Object);

        var act = async () => await service.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}