namespace CloudDocs.Application.Features.Users.UpdateUser;

/// <summary>
/// Represents the request data for update user.
/// </summary>
/// <param name="FullName">The full name.</param>
/// <param name="Email">The email.</param>
/// <param name="Password">The password.</param>
/// <param name="DepartmentId">The department.</param>
/// <param name="RoleId">The role id identifier.</param>
public sealed record UpdateUserRequest(
    string FullName,
    string Email,
    string? Password,
    Guid? DepartmentId,
    Guid RoleId);