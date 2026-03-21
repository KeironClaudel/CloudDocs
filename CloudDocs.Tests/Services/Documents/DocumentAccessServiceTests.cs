using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;
using CloudDocs.Infrastructure.Services;
using FluentAssertions;

namespace CloudDocs.Tests.Services.Documents;

public class DocumentAccessServiceTests
{
    private readonly DocumentAccessService _service = new();

    [Fact]
    public void CanAccessDocument_ShouldReturnTrue_WhenUserIsAdmin()
    {
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin User",
            Email = "admin@test.com",
            Role = new Role { Name = "Admin" },
            Department = "Finance"
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            AccessLevel = DocumentAccessLevel.AdminOnly,
            Department = "HR"
        };

        var result = _service.CanAccessDocument(adminUser, document);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanAccessDocument_ShouldReturnTrue_WhenDocumentIsInternalPublic()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Normal User",
            Email = "user@test.com",
            Role = new Role { Name = "User" },
            Department = "Finance"
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            AccessLevel = DocumentAccessLevel.InternalPublic
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanAccessDocument_ShouldReturnFalse_WhenDocumentIsAdminOnly_AndUserIsNotAdmin()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Normal User",
            Email = "user@test.com",
            Role = new Role { Name = "User" },
            Department = "Finance"
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            AccessLevel = DocumentAccessLevel.AdminOnly
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanAccessDocument_ShouldReturnTrue_WhenOwnerOnly_AndUserIsOwner()
    {
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FullName = "Owner User",
            Email = "owner@test.com",
            Role = new Role { Name = "User" }
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = userId,
            AccessLevel = DocumentAccessLevel.OwnerOnly
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanAccessDocument_ShouldReturnFalse_WhenOwnerOnly_AndUserIsNotOwner()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Another User",
            Email = "another@test.com",
            Role = new Role { Name = "User" }
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            AccessLevel = DocumentAccessLevel.OwnerOnly
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanAccessDocument_ShouldReturnTrue_WhenDepartmentOnly_AndDepartmentMatches()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Finance User",
            Email = "finance@test.com",
            Role = new Role { Name = "User" },
            Department = "Finance"
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            AccessLevel = DocumentAccessLevel.DepartmentOnly,
            Department = "Finance"
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanAccessDocument_ShouldReturnFalse_WhenDepartmentOnly_AndDepartmentDoesNotMatch()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "HR User",
            Email = "hr@test.com",
            Role = new Role { Name = "User" },
            Department = "HR"
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = Guid.NewGuid(),
            AccessLevel = DocumentAccessLevel.DepartmentOnly,
            Department = "Finance"
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeFalse();
    }
}