using CloudDocs.Application.Features.Documents.Versions.Common;

namespace CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;

/// <summary>
/// Defines the contract for get document versions operations.
/// </summary>
public interface IGetDocumentVersionsService
{
    /// <summary>
    /// Gets the item by document id.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document version response list.</returns>
    Task<List<DocumentVersionResponse>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}