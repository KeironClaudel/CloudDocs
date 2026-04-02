using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.GetDocumentTypeById;

public class GetDocumentTypeByIdService : IGetDocumentTypeByIdService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;

    public GetDocumentTypeByIdService(IDocumentTypeRepository documentTypeRepository)
    {
        _documentTypeRepository = documentTypeRepository;
    }

    public async Task<DocumentTypeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var x = await _documentTypeRepository.GetByIdAsync(id, cancellationToken);
        if (x is null) return null;

        return new DocumentTypeResponse(
            x.Id,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt);
    }
}