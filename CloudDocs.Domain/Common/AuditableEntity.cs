namespace CloudDocs.Domain.Common;

/// <summary>
/// Provides shared members for auditable entity.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets the updated at.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}