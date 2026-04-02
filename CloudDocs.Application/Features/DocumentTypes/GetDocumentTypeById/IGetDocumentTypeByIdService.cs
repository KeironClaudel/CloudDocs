using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.GetDocumentTypeById;

public interface IGetDocumentTypeByIdService
{
    Task<DocumentTypeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
