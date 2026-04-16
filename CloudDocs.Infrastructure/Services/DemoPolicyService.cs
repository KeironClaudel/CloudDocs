using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using CloudDocs.Domain.Entities;
using Microsoft.Extensions.Options;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Enforces application restrictions for the public demo environment.
/// </summary>
public class DemoPolicyService : IDemoPolicyService
{
    private readonly DemoSettings _settings;

    public DemoPolicyService(IOptions<DemoSettings> options)
    {
        _settings = options.Value;
    }

    public bool IsDemoUser(User user)
    {
        if (!_settings.Enabled)
            return false;

        if (string.IsNullOrWhiteSpace(_settings.DemoUserEmail))
            return false;

        return string.Equals(
            user.Email?.Trim(),
            _settings.DemoUserEmail.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    public Task ValidateUploadAsync(
        User user,
        string contentType,
        long fileSize,
        int currentDocumentCount,
        CancellationToken cancellationToken = default)
    {
        if (!IsDemoUser(user))
            return Task.CompletedTask;

        if (_settings.PdfOnly &&
            !string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("The public demo only allows PDF files.");
        }

        if (fileSize > _settings.MaxDemoFileSizeInBytes)
        {
            throw new BadRequestException(
                $"The public demo only allows files up to {_settings.MaxDemoFileSizeInBytes / 1024 / 1024} MB.");
        }

        if (currentDocumentCount >= _settings.MaxDocumentsPerDemoUser)
        {
            throw new BadRequestException(
                $"The public demo allows a maximum of {_settings.MaxDocumentsPerDemoUser} uploaded documents.");
        }

        return Task.CompletedTask;
    }

    public Task ValidateSendEmailAsync(
    User user,
    int currentEmailCount,
    CancellationToken cancellationToken = default)
    {
        if (!IsDemoUser(user))
            return Task.CompletedTask;

        if (currentEmailCount >= _settings.MaxEmailsPerDemoUser)
        {
            throw new BadRequestException(
                $"The public demo allows a maximum of {_settings.MaxEmailsPerDemoUser} sent emails.");
        }

        return Task.CompletedTask;
    }
}