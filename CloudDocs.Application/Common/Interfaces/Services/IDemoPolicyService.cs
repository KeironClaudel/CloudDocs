using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Provides operations for enforcing public demo restrictions.
/// </summary>
public interface IDemoPolicyService
{
    /// <summary>
    /// Determines whether the specified user is the configured demo user.
    /// </summary>
    bool IsDemoUser(User user);

    /// <summary>
    /// Validates upload rules for the current user in demo mode.
    /// Throws an exception when the action is not allowed.
    /// </summary>
    Task ValidateUploadAsync(
        User user,
        string contentType,
        long fileSize,
        int currentDocumentCount,
        CancellationToken cancellationToken = default);

    Task ValidateSendEmailAsync(
    User user,
    int currentEmailCount,
    CancellationToken cancellationToken = default);
}