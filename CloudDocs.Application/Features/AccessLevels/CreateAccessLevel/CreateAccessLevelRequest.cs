namespace CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;

public sealed record CreateAccessLevelRequest(
    string Code,
    string Name,
    string? Description);