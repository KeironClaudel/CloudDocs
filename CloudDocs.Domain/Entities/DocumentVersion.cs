using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a document version entity.
/// </summary>
public class DocumentVersion : AuditableEntity
{
    /// <summary>
    /// Gets or sets the document id.
    /// </summary>
    public Guid DocumentId { get; set; }
    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    public Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int VersionNumber { get; set; }
    /// <summary>
    /// Gets or sets the stored file name.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the storage path.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the uploaded by user id.
    /// </summary>
    public Guid UploadedByUserId { get; set; }
    /// <summary>
    /// Gets or sets the uploaded by user.
    /// </summary>
    public User UploadedByUser { get; set; } = null!;
}