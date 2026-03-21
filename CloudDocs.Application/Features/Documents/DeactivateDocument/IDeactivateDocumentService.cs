namespace CloudDocs.Application.Features.Documents.DeactivateDocument;

public interface IDeactivateDocumentService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}