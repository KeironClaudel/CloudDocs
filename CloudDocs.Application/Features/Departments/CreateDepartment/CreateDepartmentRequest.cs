namespace CloudDocs.Application.Features.Departments.CreateDepartment;

public sealed record CreateDepartmentRequest(
    string Name,
    string? Description);