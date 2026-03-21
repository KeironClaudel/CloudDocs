namespace CloudDocs.Application.Features.Users.UpdateUser;

public sealed record UpdateUserRequest(
    string FullName,
    string Email,
    string? Department,
    Guid RoleId);