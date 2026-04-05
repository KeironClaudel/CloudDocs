using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a user entity.
/// </summary>
public class User : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>
    /// Gets or sets the role id.
    /// </summary>
    public Guid RoleId { get; set; }
    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; set; }
    /// <summary>
    /// Gets or sets the lockout end utc.
    /// </summary>
    public DateTime? LockoutEndUtc { get; set; }
    /// <summary>
    /// Gets or sets the last activity at utc.
    /// </summary>
    public DateTime? LastActivityAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the uploaded documents.
    /// </summary>
    public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
}