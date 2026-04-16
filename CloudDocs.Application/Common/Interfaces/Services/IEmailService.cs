namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Provides operations for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The subject.</param>
    /// <param name="body">The message body.</param>
    /// <param name="attachmentFileName">The optional attachment file name.</param>
    /// <param name="attachmentStream">The optional attachment stream.</param>
    /// <param name="attachmentContentType">The optional attachment content type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendAsync(
        string to,
        string subject,
        string body,
        string? attachmentFileName = null,
        Stream? attachmentStream = null,
        string? attachmentContentType = null,
        CancellationToken cancellationToken = default);
}