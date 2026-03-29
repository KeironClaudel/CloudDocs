using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides operations for document access.
/// </summary>
public class DocumentAccessService : IDocumentAccessService
{
    /// <summary>
    /// Validates if the user can access the document based in current role.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="document">The document.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
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