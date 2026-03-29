namespace CloudDocs.Application.Features.Auth.ChangePassword;

/// <summary>
/// Represents the request data for change password.
/// </summary>
/// <param name="CurrentPassword">The current password.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);