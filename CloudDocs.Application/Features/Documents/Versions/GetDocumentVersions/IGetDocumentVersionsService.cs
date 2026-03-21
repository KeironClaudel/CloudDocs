using CloudDocs.Application.Features.Documents.Versions.Common;

namespace CloudDocs.Application.Features.Documents.Versions.GetDocumentVersions;

public interface IGetDocumentVersionsService
{
    Task<List<DocumentVersionResponse>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}