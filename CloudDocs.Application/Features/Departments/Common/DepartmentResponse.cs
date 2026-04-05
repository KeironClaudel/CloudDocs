namespace CloudDocs.Application.Features.Departments.Common;

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);