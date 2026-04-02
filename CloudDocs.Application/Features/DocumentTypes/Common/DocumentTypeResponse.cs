namespace CloudDocs.Application.Features.DocumentTypes.Common;

public sealed record DocumentTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);