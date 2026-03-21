using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;

namespace CloudDocs.Infrastructure.Services;

public class DocumentAccessService : IDocumentAccessService
{
    public bool CanAccessDocument(User currentUser, Document document)
    {
        var isAdmin = string.Equals(currentUser.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase);

        if (isAdmin)
            return true;

        return document.AccessLevel switch
        {
            DocumentAccessLevel.InternalPublic => true,
            DocumentAccessLevel.Private => document.UploadedByUserId == currentUser.Id,
            DocumentAccessLevel.AdminOnly => false,
            DocumentAccessLevel.OwnerOnly => document.UploadedByUserId == currentUser.Id,
            DocumentAccessLevel.DepartmentOnly =>
                !string.IsNullOrWhiteSpace(document.Department) &&
                !string.IsNullOrWhiteSpace(currentUser.Department) &&
                string.Equals(document.Department, currentUser.Department, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}