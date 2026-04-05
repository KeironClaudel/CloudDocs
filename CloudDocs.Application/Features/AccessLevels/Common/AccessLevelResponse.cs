namespace CloudDocs.Application.Features.AccessLevels.Common;

public sealed record AccessLevelResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);