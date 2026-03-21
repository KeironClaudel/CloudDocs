using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

public class Category : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}