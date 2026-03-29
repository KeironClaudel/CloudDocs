using CloudDocs.Domain.Enums;

namespace CloudDocs.Application.Features.Documents.Common;

/// <summary>
/// Represents the response data for document.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="OriginalFileName">The original file name.</param>
/// <param name="StoredFileName">The stored file name.</param>
/// <param name="FileExtension">The file extension.</param>
/// <param name="ContentType">The content type.</param>
/// <param name="FileSize">The file size.</param>
/// <param name="CategoryId">The category id identifier.</param>
/// <param name="CategoryName">The category name.</param>
/// <param name="UploadedByUserId">The uploaded by user id identifier.</param>
/// <param name="UploadedByUserName">The uploaded by user name.</param>
/// <param name="Month">The month.</param>
/// <param name="Year">The year.</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="ExpirationDate">The expiration date.</param>
/// <param name="ExpirationDatePendingDefinition">The expiration date pending definition.</param>
/// <param name="AccessLevel">The access level.</param>
/// <param name="Department">The department.</param>
/// <param name="IsActive">The is active.</param>
/// <param name="CreatedAt">The created at.</param>
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