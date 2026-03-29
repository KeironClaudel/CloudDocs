namespace CloudDocs.Application.Common.Models;

/// <summary>
/// Represents configuration settings for jwt.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Gets or sets the secret.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether suer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the expiration minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; }
}