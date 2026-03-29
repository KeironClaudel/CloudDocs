namespace CloudDocs.Application.Features.Auth.RefreshToken;

/// <summary>
/// Represents the response data for refresh token.
/// </summary>
/// <param name="AccessToken">The access token.</param>
/// <param name="RefreshToken">The refresh token.</param>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);