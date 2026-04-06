namespace CloudDocs.Application.Features.Auth.Me;

public sealed record CurrentUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role);