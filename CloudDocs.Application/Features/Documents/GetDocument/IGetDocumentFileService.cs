using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.GetDocumentFile;

public interface IGetDocumentFileService
{
    Task<(Stream? Stream, string FileName, string ContentType)?> GetFileAsync(
        User currentUser,
        Guid id,
        string action,
        Guid? actorUserId = null,
        CancellationToken cancellationToken = default);
}