using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a audit log entity.
/// </summary>
public class AuditLog : AuditableEntity
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid? UserId { get; set; }
    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the module.
    /// </summary>
    public string Module { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the entity id.
    /// </summary>
    public string? EntityId { get; set; }
    /// <summary>
    /// Gets or sets the details.
    /// </summary>
    public string? Details { get; set; }
    /// <summary>
    /// Gets or sets the ip address.
    /// </summary>
    public string? IpAddress { get; set; }
}