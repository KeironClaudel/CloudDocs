using CloudDocs.Application.Features.Documents.Versions.Common;

namespace CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;

/// <summary>
/// Defines the contract for upload document version operations.
/// </summary>
public interface IUploadDocumentVersionService
{
    /// <summary>
    /// Uploads.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="currentUserId">The current user id identifier.</param>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document version response.</returns>
    Task<DocumentVersionResponse> UploadAsync(
        Guid documentId,
        Guid currentUserId,
        Stream fileStream,
        UploadDocumentVersionRequest request,
        CancellationToken cancellationToken = default);
}