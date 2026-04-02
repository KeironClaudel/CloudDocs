using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;

public interface ICreateDocumentTypeService
{
    Task<DocumentTypeResponse> CreateAsync(CreateDocumentTypeRequest request, CancellationToken cancellationToken = default);
}