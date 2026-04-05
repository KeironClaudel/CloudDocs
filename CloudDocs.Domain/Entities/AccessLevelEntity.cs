using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a configurable access level used to control document visibility.
/// </summary>
public class AccessLevelEntity : SoftDeletableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
