using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a configurable document type used to classify uploaded documents.
/// </summary>
public class DocumentTypeEntity : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}