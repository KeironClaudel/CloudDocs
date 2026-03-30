using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentFile;

/// <summary>
/// Defines the contract for get document file operations.
/// </summary>
public interface IGetDocumentFileService
{
    /// <summary>
    /// Gets the file.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="documentId">The identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="actorUserId">The actor user identifier.</param>
    /// <param name="versionId">The document version identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the (stream?stream,string file name,string content type) when available; otherwise, null.</returns>
    Task<(Stream? Stream, string FileName, string ContentType)?> GetFileAsync(
        User currentUser,
        Guid documentId,
        string action,
        Guid? actorUserId = null,
        Guid? versionId = null,
        CancellationToken cancellationToken = default);
}