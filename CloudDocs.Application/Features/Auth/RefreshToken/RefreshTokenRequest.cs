namespace CloudDocs.Application.Features.Auth.RefreshToken;

/// <summary>
/// Represents the request data for refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token.</param>
public sealed record RefreshTokenRequest(string RefreshToken);