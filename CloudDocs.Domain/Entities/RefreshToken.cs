using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents a refresh token entity.
/// </summary>
public class RefreshToken : BaseEntity
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
    /// Gets or sets the revoked at.
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}