namespace CloudDocs.Application.Features.Clients.Common;

public sealed record ClientResponse(
    Guid Id,
    string Name,
    string? LegalName,
    string? Identification,
    string? Email,
    string? Phone,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt);