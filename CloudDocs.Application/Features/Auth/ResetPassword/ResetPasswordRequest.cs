namespace CloudDocs.Application.Features.Auth.ResetPassword;

/// <summary>
/// Represents the request data for reset password.
/// </summary>
/// <param name="Token">The token.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);