using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.GetDocumentTypes;

public class GetDocumentTypesService : IGetDocumentTypesService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;

    public GetDocumentTypesService(IDocumentTypeRepository documentTypeRepository)
    {
        _documentTypeRepository = documentTypeRepository;
    }

    public async Task<List<DocumentTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _documentTypeRepository.GetAllAsync(cancellationToken);

        return items.Select(x => new DocumentTypeResponse(
            x.Id,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}