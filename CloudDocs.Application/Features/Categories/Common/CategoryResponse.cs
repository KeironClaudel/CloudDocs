namespace CloudDocs.Application.Features.Categories.Common;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
    );