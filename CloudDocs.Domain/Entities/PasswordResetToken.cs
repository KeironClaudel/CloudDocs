using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a password reset token entity.
/// </summary>
public class PasswordResetToken : BaseEntity
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the expires at.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// Gets or sets the used at.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether used.
    /// </summary>
    public bool IsUsed => UsedAt.HasValue;
}