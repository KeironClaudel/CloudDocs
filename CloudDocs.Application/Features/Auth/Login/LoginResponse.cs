namespace CloudDocs.Application.Features.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Email,
    string Role
    );