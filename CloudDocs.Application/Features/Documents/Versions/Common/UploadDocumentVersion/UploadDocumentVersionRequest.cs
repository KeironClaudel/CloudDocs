namespace CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;

public sealed record UploadDocumentVersionRequest(
    string OriginalFileName,
    string ContentType,
    long FileSize
    );