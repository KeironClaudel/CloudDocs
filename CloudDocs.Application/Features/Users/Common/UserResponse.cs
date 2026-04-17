namespace CloudDocs.Application.Features.Users.Common;

/// <summary>
/// Represents the response data for user.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid RoleId,
    string RoleName,
    bool IsActive,
    DateTime CreatedAt)
{
    /// <summary>
    /// Backwards-compatible department name property.
    /// </summary>
    public string? Department => DepartmentName;

    /// <summary>
    /// Backwards-compatible role name property.
    /// </summary>
    public string Role => RoleName;
}
