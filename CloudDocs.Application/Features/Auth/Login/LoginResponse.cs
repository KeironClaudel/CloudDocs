namespace CloudDocs.Application.Features.Auth.Login;

/// <summary>
/// Represents the response data for login.
/// </summary>
/// <param name="AccessToken">The access token.</param>
/// <param name="RefreshToken">The refresh token.</param>
/// <param name="FullName">The full name.</param>
/// <param name="Email">The email.</param>
/// <param name="Role">The role.</param>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Email,
    string Role
    );