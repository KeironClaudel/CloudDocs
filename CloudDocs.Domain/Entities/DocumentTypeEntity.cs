using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a configurable document type used to classify uploaded documents.
/// </summary>
public class DocumentTypeEntity : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>
    /// Indicates whether this document type requires an expiration date or a pending definition.
    /// </summary>
    public bool RequiresExpiration { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}