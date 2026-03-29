namespace CloudDocs.Domain.Common;

/// <summary>
/// Provides shared members for base entity.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}