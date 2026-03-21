using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Documents.Versions.Common;

namespace CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;

public class GetDocumentVersionsService : IGetDocumentVersionsService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentVersionRepository _documentVersionRepository;

    public GetDocumentVersionsService(
        IDocumentRepository documentRepository,
        IDocumentVersionRepository documentVersionRepository)
    {
        _documentRepository = documentRepository;
        _documentVersionRepository = documentVersionRepository;
    }

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