namespace CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;

public sealed record UpdateDocumentTypeRequest(
    string Name,
    string? Description);