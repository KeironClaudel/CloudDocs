using CloudDocs.Domain.Common;
using CloudDocs.Domain.Enums;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a document entity.
/// </summary>
public class Document : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the stored file name.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    public long FileSize { get; set; }
    /// <summary>
    /// Gets or sets the storage path.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category id.
    /// </summary>
    public Guid CategoryId { get; set; }
    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the uploaded by user id.
    /// </summary>
    public Guid UploadedByUserId { get; set; }
    /// <summary>
    /// Gets or sets the uploaded by user.
    /// </summary>
    public User UploadedByUser { get; set; } = null!;

    /// <summary>
    /// Gets or sets the month.
    /// </summary>
    public int Month { get; set; }
    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.General;
    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    /// <summary>
    /// Gets or sets the expiration date pending definition.
    /// </summary>
    public bool ExpirationDatePendingDefinition { get; set; }

    /// <summary>
    /// Gets or sets the access level.
    /// </summary>
    public DocumentAccessLevel AccessLevel { get; set; } = DocumentAccessLevel.InternalPublic;
    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets the versions.
    /// </summary>
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}