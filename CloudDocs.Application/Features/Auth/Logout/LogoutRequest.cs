namespace CloudDocs.Application.Features.Auth.Logout;

/// <summary>
/// Represents the request data for logout.
/// </summary>
/// <param name="RefreshToken">The refresh token.</param>
public sealed record LogoutRequest(string RefreshToken);