namespace CloudDocs.Application.Features.Documents.Versions.Common;

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