using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a business department that can own users and be granted access to documents.
/// </summary>
public class Department : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<DocumentDepartment> DocumentDepartments { get; set; } = new List<DocumentDepartment>();
}
