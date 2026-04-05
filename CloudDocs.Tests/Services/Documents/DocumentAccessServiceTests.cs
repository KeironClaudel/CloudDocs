using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;
using CloudDocs.Infrastructure.Services;
using FluentAssertions;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for document access service.
/// </summary>
public class DocumentAccessServiceTests
{
    private readonly DocumentAccessService _service = new();

    /// <summary>
    /// Verifies that can access document should return true when user is admin.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "ADMIN_ONLY", Name = "Admin Only" },
            Department = "HR"
        };

        var result = _service.CanAccessDocument(adminUser, document);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that can access document should return true when document is internal public.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "INTERNAL_PUBLIC", Name = "Internal Public" }
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that can access document should return false when document is admin only and user is not admin.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "ADMIN_ONLY", Name = "Admin Only" }
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that can access document should return true when owner only and user is owner.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "OWNER_ONLY", Name = "Owner Only" }
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that can access document should return false when owner only and user is not owner.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "OWNER_ONLY", Name = "Owner Only" }
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that can access document should return true when department only and department matches.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "DEPARTMENT_ONLY", Name = "Department Only" },
            Department = "Finance"
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that can access document should return false when department only and department does not match.
    /// </summary>
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
            AccessLevel = new AccessLevelEntity { Code = "DEPARTMENT_ONLY", Name = "Department Only" },
            Department = "Finance"
        };

        var result = _service.CanAccessDocument(user, document);

        result.Should().BeFalse();
    }
}