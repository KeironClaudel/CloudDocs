using CloudDocs.Application.Features.Documents.Versions.Common;

namespace CloudDocs.Application.Features.Documents.Versions.UploadDocumentVersion;

public interface IUploadDocumentVersionService
{
    Task<DocumentVersionResponse> UploadAsync(
        Guid documentId,
        Guid currentUserId,
        Stream fileStream,
        UploadDocumentVersionRequest request,
        CancellationToken cancellationToken = default);
}