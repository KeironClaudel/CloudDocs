using CloudDocs.Application.Features.Documents.Common;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

/// <summary>
/// Defines the contract for upload document operations.
/// </summary>
public interface IUploadDocumentService
{
    /// <summary>
    /// Uploads.
    /// </summary>
    /// <param name="currentUserId">The current user id identifier.</param>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document response.</returns>
    Task<DocumentResponse> UploadAsync(
        Guid currentUserId,
        Stream fileStream,
        UploadDocumentRequest request,
        CancellationToken cancellationToken = default);
}