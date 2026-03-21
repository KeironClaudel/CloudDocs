using CloudDocs.Domain.Enums;

namespace CloudDocs.Application.Features.Documents.Common;

public sealed record DocumentResponse(
    Guid Id,
    string OriginalFileName,
    string StoredFileName,
    string FileExtension,
    string ContentType,
    long FileSize,
    Guid CategoryId,
    string CategoryName,
    Guid UploadedByUserId,
    string UploadedByUserName,
    int Month,
    int Year,
    DocumentType DocumentType,
    DateTime? ExpirationDate,
    bool ExpirationDatePendingDefinition,
    DocumentAccessLevel AccessLevel,
    string? Department,
    bool IsActive,
    DateTime CreatedAt);