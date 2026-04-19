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
            var isAdmin = string.Equals(currentUser.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);

            if (isAdmin) return true;

            var accessCode = document.AccessLevel?.Code?.Trim().ToUpperInvariant();
            var userDepartmentId = currentUser.DepartmentId;
            var userDept = currentUser.Department?.Name?.Trim();
            return accessCode switch
            {
                "INTERNAL_PUBLIC" => true,
                "ADMIN_ONLY" => false,
                "OWNER_ONLY" => document.UploadedByUserId == currentUser.Id,
                "DEPARTMENT_ONLY" => 
                    document.DocumentDepartments != null &&
                    (
                        (userDepartmentId.HasValue &&
                         document.DocumentDepartments.Any(dd => dd.DepartmentId == userDepartmentId.Value)) ||
                        (!userDepartmentId.HasValue &&
                         !string.IsNullOrWhiteSpace(userDept) &&
                         document.DocumentDepartments.Any(dd => string.Equals(dd.Department?.Name?.Trim(), userDept, StringComparison.OrdinalIgnoreCase)))
                    ),
                _ => false
            };
    }
}
