namespace CloudDocs.Application.Features.Documents.SendDocumentToClient;

public interface ISendDocumentToClientService
{
    Task ExecuteAsync(
        Guid documentId,
        Guid currentUserId,
        SendDocumentToClientRequest request,
        CancellationToken cancellationToken = default);
}