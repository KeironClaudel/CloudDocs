namespace CloudDocs.Application.Features.Clients.CreateClient;

public sealed record CreateClientRequest(
    string Name,
    string? LegalName,
    string? Identification,
    string? Email,
    string? Phone,
    string? Notes);