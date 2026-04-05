namespace CloudDocs.Application.Features.Departments.UpdateDepartment;

public sealed record UpdateDepartmentRequest(
    string Name,
    string? Description);