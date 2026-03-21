using CloudDocs.Application.Features.Documents.Common;

namespace CloudDocs.Application.Features.Documents.UploadDocument;

public interface IUploadDocumentService
{
    Task<DocumentResponse> UploadAsync(
        Guid currentUserId,
        Stream fileStream,
        UploadDocumentRequest request,
        CancellationToken cancellationToken = default);
}