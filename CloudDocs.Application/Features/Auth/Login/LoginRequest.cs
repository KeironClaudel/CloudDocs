namespace CloudDocs.Application.Features.Auth.Login;

/// <summary>
/// Represents the request data for login.
/// </summary>
/// <param name="Email">The email.</param>
/// <param name="Password">The password.</param>
public sealed record LoginRequest(
    string Email,
    string Password);