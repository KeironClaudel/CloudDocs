namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Represents a service for sending emails.
/// </summary>

public interface IEmailService
{
    /// <summary>Asynchronously sends an email message to the specified recipient with the given subject and body.
    /// </summary>
    /// <param name="to">The email address of the recipient. Cannot be null or empty.</param>
    /// <param name="subject">The subject line of the email message. Cannot be null.</param>
    /// <param name="body">The body content of the email message. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
