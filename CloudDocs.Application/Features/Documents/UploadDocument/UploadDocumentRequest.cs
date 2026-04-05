using CloudDocs.Domain.Enums;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

/// <summary>
/// Represents the request data for upload document.
/// </summary>
/// <param name="OriginalFileName">The original file name.</param>
/// <param name="ContentType">The content type.</param>
/// <param name="FileSize">The file size.</param>
/// <param name="CategoryId">The category id identifier.</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="ExpirationDate">The expiration date.</param>
/// <param name="ExpirationDatePendingDefinition">The expiration date pending definition.</param>
/// <param name="AccessLevel">The access level.</param>
/// <param name="Department">The department.</param>
public sealed record UploadDocumentRequest(
    string OriginalFileName,
    string ContentType,
    long FileSize,
    Guid CategoryId,
    Guid DocumentTypeId,
    DateTime? ExpirationDate,
    bool ExpirationDatePendingDefinition,
    Guid AccessLevelId,
    string? Department);