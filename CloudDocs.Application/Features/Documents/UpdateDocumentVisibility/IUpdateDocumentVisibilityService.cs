namespace CloudDocs.Application.Features.Documents.UpdateDocumentVisibility;

public interface IUpdateDocumentVisibilityService
{
    Task<bool> UpdateAsync(Guid documentId, UpdateDocumentVisibilityRequest request, CancellationToken cancellationToken = default);
}