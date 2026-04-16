namespace CloudDocs.Application.Features.Documents.SendDocumentToClient;

/// <summary>
/// Represents the request to send a document to a client.
/// </summary>
public sealed record SendDocumentToClientRequest(
    string? Subject,
    string? Message);