namespace CloudDocs.Application.Common.Models;

/// <summary>
/// Represents a queued audit log request.
/// </summary>
public sealed record AuditLogRequest(
    Guid? UserId,
    string Action,
    string Module,
    string EntityName,
    string? EntityId = null,
    string? Details = null,
    string? IpAddress = null);
