using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Application.Features.Documents.SearchDocuments;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Documents;

/// <summary>
/// Contains tests for search documents service.
/// </summary>
public class SearchDocumentsServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock = new();
    private readonly Mock<IDocumentAccessService> _documentAccessServiceMock = new();

    private SearchDocumentsService CreateService()
    {
        return new SearchDocumentsService(
            _documentRepositoryMock.Object,
            _documentAccessServiceMock.Object);
    }

    /// <summary>
    /// Verifies that search documents should return empty list when no documents exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyList_WhenNoDocumentsExist()
    {
        var currentUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            FullName = "Test User",
            IsActive = true
        };

        var request = new SearchDocumentsRequest(
            Name: "test",
            CategoryId: null,
            Month: null,
            Year: null,
            DocumentTypeId: null,
            ExpirationPendingDefinition: null);

        var pagedResult = new PagedResult<Document>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
            Items = new List<Document>()
        };

        _documentRepositoryMock
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var service = CreateService();
        var result = await service.SearchAsync(currentUser, request);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that search documents should filter documents by user access.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SearchAsync_ShouldFilterDocumentsByAccess_WhenSearchingDocuments()
    {
        var currentUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            FullName = "Test User",
            IsActive = true
        };

        var request = new SearchDocumentsRequest(
            Name: "test",
            CategoryId: null,
            Month: null,
            Year: null,
            DocumentTypeId: null,
            ExpirationPendingDefinition: null);

        var accessibleDoc = new Document
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "Accessible.pdf",
            StoredFileName = "stored-1.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            CategoryId = Guid.NewGuid(),
            Category = new Category { Name = "Finance" },
            UploadedByUserId = currentUser.Id,
            UploadedByUser = currentUser,
            Month = 1,
            Year = 2024,
            DocumentTypeId = Guid.NewGuid(),
            DocumentType = new DocumentTypeEntity { Name = "Invoice" },
            AccessLevelId = Guid.NewGuid(),
            AccessLevel = new AccessLevelEntity { Name = "Internal Public", Code = "INTERNAL_PUBLIC" },
            DocumentDepartments = new List<DocumentDepartment>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var restrictedDoc = new Document
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "Restricted.pdf",
            StoredFileName = "stored-2.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSize = 2048,
            CategoryId = Guid.NewGuid(),
            Category = new Category { Name = "HR" },
            UploadedByUserId = Guid.NewGuid(),
            UploadedByUser = new User { FullName = "Other User" },
            Month = 1,
            Year = 2024,
            DocumentTypeId = Guid.NewGuid(),
            DocumentType = new DocumentTypeEntity { Name = "Policy" },
            AccessLevelId = Guid.NewGuid(),
            AccessLevel = new AccessLevelEntity { Name = "Admin Only", Code = "ADMIN_ONLY" },
            DocumentDepartments = new List<DocumentDepartment>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var pagedResult = new PagedResult<Document>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 2,
            Items = new List<Document> { accessibleDoc, restrictedDoc }
        };

        _documentRepositoryMock
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _documentAccessServiceMock
            .Setup(x => x.CanAccessDocument(currentUser, accessibleDoc))
            .Returns(true);

        _documentAccessServiceMock
            .Setup(x => x.CanAccessDocument(currentUser, restrictedDoc))
            .Returns(false);

        var service = CreateService();
        var result = await service.SearchAsync(currentUser, request);

        result.Items.Should().HaveCount(1);
        result.Items[0].OriginalFileName.Should().Be("Accessible.pdf");
    }

    /// <summary>
    /// Verifies that search documents should map document properties correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SearchAsync_ShouldMapDocumentPropertiesCorrectly_WhenSearchingDocuments()
    {
        var currentUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            FullName = "Test User",
            IsActive = true
        };

        var request = new SearchDocumentsRequest(
            Name: "test",
            CategoryId: null,
            Month: null,
            Year: null,
            DocumentTypeId: null,
            ExpirationPendingDefinition: null);

        var category = new Category { Id = Guid.NewGuid(), Name = "Finance" };
        var docType = new DocumentTypeEntity { Id = Guid.NewGuid(), Name = "Invoice" };
        var accessLevel = new AccessLevelEntity { Id = Guid.NewGuid(), Name = "Public", Code = "PUBLIC" };
        var dept = new Department { Id = Guid.NewGuid(), Name = "Finance Dept" };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "Invoice-2024.pdf",
            StoredFileName = "stored-inv-2024.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSize = 5120,
            CategoryId = category.Id,
            Category = category,
            UploadedByUserId = currentUser.Id,
            UploadedByUser = currentUser,
            Month = 1,
            Year = 2024,
            DocumentTypeId = docType.Id,
            DocumentType = docType,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            AccessLevelId = accessLevel.Id,
            AccessLevel = accessLevel,
            DocumentDepartments = new List<DocumentDepartment>
            {
                new DocumentDepartment { DepartmentId = dept.Id, Department = dept }
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var pagedResult = new PagedResult<Document>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = new List<Document> { document }
        };

        _documentRepositoryMock
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _documentAccessServiceMock
            .Setup(x => x.CanAccessDocument(currentUser, document))
            .Returns(true);

        var service = CreateService();
        var result = await service.SearchAsync(currentUser, request);

        result.Items.Should().HaveCount(1);
        var mappedDoc = result.Items[0];
        mappedDoc.Id.Should().Be(document.Id);
        mappedDoc.OriginalFileName.Should().Be("Invoice-2024.pdf");
        mappedDoc.CategoryName.Should().Be("Finance");
        mappedDoc.DocumentTypeName.Should().Be("Invoice");
        mappedDoc.AccessLevelName.Should().Be("Public");
        mappedDoc.VisibleDepartments.Should().HaveCount(1);
        mappedDoc.VisibleDepartments[0].Name.Should().Be("Finance Dept");
    }
}
