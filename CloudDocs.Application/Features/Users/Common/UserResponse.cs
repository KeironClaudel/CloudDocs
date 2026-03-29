namespace CloudDocs.Application.Features.Users.Common;

/// <summary>
/// Represents the response data for user.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="FullName">The full name.</param>
/// <param name="Email">The email.</param>
/// <param name="Department">The department.</param>
/// <param name="Role">The role.</param>
/// <param name="IsActive">The is active.</param>
/// <param name="CreatedAt">The created at.</param>
public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Department,
    string Role,
    bool IsActive,
    DateTime CreatedAt);