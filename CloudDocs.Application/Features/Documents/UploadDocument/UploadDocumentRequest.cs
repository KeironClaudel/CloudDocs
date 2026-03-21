using CloudDocs.Domain.Enums;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

public sealed record UploadDocumentRequest(
    string OriginalFileName,
    string ContentType,
    long FileSize,
    Guid CategoryId,
    DocumentType DocumentType,
    DateTime? ExpirationDate,
    bool ExpirationDatePendingDefinition,
    DocumentAccessLevel AccessLevel,
    string? Department);