namespace CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;

public sealed record UpdateAccessLevelRequest(
    string Name,
    string? Description);
