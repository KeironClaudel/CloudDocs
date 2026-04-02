using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.GetDocumentTypes;

public interface IGetDocumentTypesService
{
    Task<List<DocumentTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}