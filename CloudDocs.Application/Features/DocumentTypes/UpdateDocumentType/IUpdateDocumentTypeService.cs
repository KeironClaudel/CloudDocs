using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;

public interface IUpdateDocumentTypeService
{
    Task<DocumentTypeResponse?> UpdateAsync(Guid id, UpdateDocumentTypeRequest request, CancellationToken cancellationToken = default);
}