using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a role entity.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the users.
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}