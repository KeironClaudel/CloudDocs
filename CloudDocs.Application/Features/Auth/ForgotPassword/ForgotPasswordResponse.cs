namespace CloudDocs.Application.Features.Auth.ForgotPassword;

/// <summary>
/// Represents the response data for forgot password.
/// </summary>
/// <param name="Message">The message.</param>
/// <param name="ResetToken">The reset token.</param>
public sealed record ForgotPasswordResponse(
    string Message);