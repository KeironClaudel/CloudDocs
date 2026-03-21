using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentById;

public interface IGetDocumentByIdService
{
    Task<DocumentResponse?> GetByIdAsync(User currentUser, Guid id, CancellationToken cancellationToken = default);
}