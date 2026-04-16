using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides operations for sending emails through SMTP.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailService"/> class.
    /// </summary>
    /// <param name="options">The SMTP settings.</param>
    public SmtpEmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    /// <summary>
    /// Sends an email message using SMTP.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The subject.</param>
    /// <param name="body">The body.</param>
    /// <param name="attachmentFileName">The optional attachment file name.</param>
    /// <param name="attachmentStream">The optional attachment stream.</param>
    /// <param name="attachmentContentType">The optional attachment content type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        string? attachmentFileName = null,
        Stream? attachmentStream = null,
        string? attachmentContentType = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email is required.", nameof(to));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Email subject is required.", nameof(subject));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Email body is required.", nameof(body));

        if (string.IsNullOrWhiteSpace(_settings.Host))
            throw new InvalidOperationException("SMTP host is not configured.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("SMTP from email is not configured.");

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = body
        };

        if (attachmentStream is not null && !string.IsNullOrWhiteSpace(attachmentFileName))
        {
            if (attachmentStream.CanSeek)
                attachmentStream.Position = 0;

            builder.Attachments.Add(
                attachmentFileName,
                attachmentStream,
                ContentType.Parse(attachmentContentType ?? "application/octet-stream"));
        }

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        var secureSocketOption = _settings.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await smtp.ConnectAsync(
            _settings.Host,
            _settings.Port,
            secureSocketOption,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            await smtp.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);
        }

        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}