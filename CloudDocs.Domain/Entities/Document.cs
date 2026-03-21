using CloudDocs.Domain.Common;
using CloudDocs.Domain.Enums;

namespace CloudDocs.Domain.Entities;

public class Document : SoftDeletableEntity
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;

    public int Month { get; set; }
    public int Year { get; set; }

    public DocumentType DocumentType { get; set; } = DocumentType.General;
    public DateTime? ExpirationDate { get; set; }
    public bool ExpirationDatePendingDefinition { get; set; }

    public DocumentAccessLevel AccessLevel { get; set; } = DocumentAccessLevel.InternalPublic;
    public string? Department { get; set; }

    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}