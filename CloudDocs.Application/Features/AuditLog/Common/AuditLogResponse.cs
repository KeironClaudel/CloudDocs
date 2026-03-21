namespace CloudDocs.Application.Features.AuditLogs.Common;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? UserId,
    string Action,
    string Module,
    string EntityName,
    string? EntityId,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt);