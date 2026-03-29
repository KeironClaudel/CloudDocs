using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Documents.Versions.Common;

namespace CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;

/// <summary>
/// Provides operations for get document versions.
/// </summary>
public class GetDocumentVersionsService : IGetDocumentVersionsService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentVersionRepository _documentVersionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentVersionsService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="documentVersionRepository">The document version repository.</param>
    public GetDocumentVersionsService(
        IDocumentRepository documentRepository,
        IDocumentVersionRepository documentVersionRepository)
    {
        _documentRepository = documentRepository;
        _documentVersionRepository = documentVersionRepository;
    }

    /// <summary>
    /// Gets the item by document id.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document version response list.</returns>
    public async Task<List<DocumentVersionResponse>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            return new List<DocumentVersionResponse>();

        var versions = await _documentVersionRepository.GetByDocumentIdAsync(documentId, cancellationToken);

        return versions.Select(x => new DocumentVersionResponse(
            x.Id,
            x.DocumentId,
            x.VersionNumber,
            x.StoredFileName,
            x.StoragePath,
            x.UploadedByUserId,
            x.UploadedByUser.FullName,
            x.CreatedAt)).ToList();
    }
}