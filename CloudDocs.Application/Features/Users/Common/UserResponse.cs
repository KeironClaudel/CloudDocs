namespace CloudDocs.Application.Features.Users.Common;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Department,
    string Role,
    bool IsActive,
    DateTime CreatedAt);