namespace CloudDocs.Application.Features.Email;

/// <summary>
/// Represents SMTP configuration settings.
/// </summary>
public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = "CloudDocs";
    public string FromEmail { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}