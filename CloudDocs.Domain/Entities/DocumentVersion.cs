using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

public class DocumentVersion : AuditableEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string StoredFileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;

    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;
}