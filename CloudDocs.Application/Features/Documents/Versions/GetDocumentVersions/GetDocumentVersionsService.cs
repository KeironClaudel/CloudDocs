using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Versions.Common;
using CloudDocs.Domain.Entities;

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

        return versions.Select(MapVersion).ToList();
    }

    /// <summary>
    /// Gets paged versions by document id.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged result of document version response.</returns>
    public async Task<PagedResult<DocumentVersionResponse>> GetPagedByDocumentIdAsync(
        Guid documentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            return new PagedResult<DocumentVersionResponse>
            {
                Page = page < 1 ? 1 : page,
                PageSize = Math.Clamp(pageSize, 1, 100),
                TotalCount = 0
            };
        }

        var versions = await _documentVersionRepository.GetPagedByDocumentIdAsync(documentId, page, pageSize, cancellationToken);

        return new PagedResult<DocumentVersionResponse>
        {
            Page = versions.Page,
            PageSize = versions.PageSize,
            TotalCount = versions.TotalCount,
            Items = versions.Items.Select(MapVersion).ToList()
        };
    }

    private static DocumentVersionResponse MapVersion(DocumentVersion x)
        => new(
            x.Id,
            x.DocumentId,
            x.VersionNumber,
            x.StoredFileName,
            x.StoragePath,
            x.UploadedByUserId,
            x.UploadedByUser.FullName,
            x.CreatedAt);
}
