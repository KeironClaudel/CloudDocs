using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a business client associated with company documents.
/// </summary>
public class Client : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Identification { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}