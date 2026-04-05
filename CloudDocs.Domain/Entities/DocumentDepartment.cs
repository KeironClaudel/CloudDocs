namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between documents and departments.
/// </summary>
public class DocumentDepartment
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}
