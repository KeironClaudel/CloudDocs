namespace CloudDocs.Application.Features.Documents.ReactivateDocument;

public interface IReactivateDocumentService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
