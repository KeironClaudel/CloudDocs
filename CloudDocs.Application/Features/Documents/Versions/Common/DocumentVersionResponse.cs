namespace CloudDocs.Application.Features.Documents.Versions.Common;

/// <summary>
/// Represents the response data for document version.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="DocumentId">The document id identifier.</param>
/// <param name="VersionNumber">The version number.</param>
/// <param name="StoredFileName">The stored file name.</param>
/// <param name="StoragePath">The storage path.</param>
/// <param name="UploadedByUserId">The uploaded by user id identifier.</param>
/// <param name="UploadedByUserName">The uploaded by user name.</param>
/// <param name="CreatedAt">The created at.</param>
public sealed record DocumentVersionResponse(
    Guid Id,
    Guid DocumentId,
    int VersionNumber,
    string StoredFileName,
    string StoragePath,
    Guid UploadedByUserId,
    string UploadedByUserName,
    DateTime CreatedAt
    );