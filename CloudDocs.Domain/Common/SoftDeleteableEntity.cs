namespace CloudDocs.Domain.Common;

/// <summary>
/// Provides shared members for soft deletable entity.
/// </summary>
public abstract class SoftDeletableEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Gets or sets the deleted at.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    /// <summary>
    /// Gets or sets the deleted by.
    /// </summary>
    public Guid? DeletedBy { get; set; }
}