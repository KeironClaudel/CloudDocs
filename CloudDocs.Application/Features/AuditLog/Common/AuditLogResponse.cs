namespace CloudDocs.Application.Features.AuditLogs.Common;

/// <summary>
/// Represents the response data for audit log.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="UserId">The user id identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="Module">The module.</param>
/// <param name="EntityName">The entity name.</param>
/// <param name="EntityId">The entity id identifier.</param>
/// <param name="Details">The details.</param>
/// <param name="IpAddress">The ip address.</param>
/// <param name="CreatedAt">The created at.</param>
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