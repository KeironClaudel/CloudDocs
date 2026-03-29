namespace CloudDocs.Application.Features.Auth.ForgotPassword;

/// <summary>
/// Represents the request data for forgot password.
/// </summary>
/// <param name="Email">The email.</param>
public sealed record ForgotPasswordRequest(string Email);