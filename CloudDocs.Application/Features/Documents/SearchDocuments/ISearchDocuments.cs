using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.Documents.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.SearchDocuments;

public interface ISearchDocumentsService
{
    Task<PagedResult<DocumentResponse>> SearchAsync(
        User currentUser,
        SearchDocumentsRequest request,
        CancellationToken cancellationToken = default);
}