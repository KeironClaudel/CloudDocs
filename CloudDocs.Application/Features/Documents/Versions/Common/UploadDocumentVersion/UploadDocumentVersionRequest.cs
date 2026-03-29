namespace CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;

/// <summary>
/// Represents the request data for upload document version.
/// </summary>
/// <param name="OriginalFileName">The original file name.</param>
/// <param name="ContentType">The content type.</param>
/// <param name="FileSize">The file size.</param>
public sealed record UploadDocumentVersionRequest(
    string OriginalFileName,
    string ContentType,
    long FileSize
    );