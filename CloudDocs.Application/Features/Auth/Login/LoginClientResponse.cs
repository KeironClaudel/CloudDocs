namespace CloudDocs.Application.Features.Auth.Login;

public sealed record LoginClientResponse(
    string FullName,
    string Email,
    string Role);