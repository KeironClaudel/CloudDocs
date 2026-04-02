namespace CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;

public sealed record CreateDocumentTypeRequest(
    string Name,
    string? Description);