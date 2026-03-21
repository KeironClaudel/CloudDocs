namespace CloudDocs.Application.Features.Users.CreateUser;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string? Department,
    Guid RoleId);