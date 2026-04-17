namespace CloudDocs.Application.Features.Users.CreateUser;

/// <summary>
/// Represents the request data for create user.
/// </summary>
/// <param name="FullName">The full name.</param>
/// <param name="Email">The email.</param>
/// <param name="Password">The password.</param>
/// <param name="Department">The department.</param>
/// <param name="RoleId">The role id identifier.</param>
public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    Guid? DepartmentId,
    Guid RoleId);