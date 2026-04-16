namespace CloudDocs.Application.Features.Clients.UpdateClient;

public sealed record UpdateClientRequest(
    string Name,
    string? LegalName,
    string? Identification,
    string? Email,
    string? Phone,
    string? Notes);