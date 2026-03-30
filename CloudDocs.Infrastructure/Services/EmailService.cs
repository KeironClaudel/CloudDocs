using CloudDocs.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides functionality for sending email messages asynchronously.
/// </summary>
/// <remarks>This implementation is intended for scenarios where email sending is simulated, such as during
/// development or testing. No actual email is sent.</remarks>

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
   /// Asynchronously simulates sending an email message to the specified recipient.
    /// </summary>
    /// <remarks>This method does not actually send an email but logs the email details for testing or
    /// development purposes. No email is delivered to the specified recipient.</remarks>
    /// <param name="to">The email address of the recipient. Cannot be null or empty.</param>
    /// <param name="subject">The subject line of the email message. Cannot be null.</param>
    /// <param name="body">The body content of the email message. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fake email sent to {To}. Subject: {Subject}. Body: {Body}",
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}
