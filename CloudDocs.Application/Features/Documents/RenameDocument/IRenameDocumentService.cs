namespace CloudDocs.Application.Features.Documents.RenameDocument;

public interface IRenameDocumentService
{
    Task<bool> RenameAsync(Guid id, RenameDocumentRequest request, CancellationToken cancellationToken = default);
}