namespace CloudDocs.Application.Common.Models;

/// <summary>
/// Represents configuration settings for frontend settings.
/// </summary>
public class FrontendSettings
{
    /// <summary>
    /// Gets or sets the section name for frontend settings in the configuration.
    /// </summary>
    public const string SectionName = "Frontend";

    /// <summary>
    /// Gets or sets the base URL of the frontend application, used for constructing links in emails and other communications.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}
