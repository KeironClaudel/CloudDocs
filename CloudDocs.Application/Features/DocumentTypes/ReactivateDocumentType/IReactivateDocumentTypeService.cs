namespace CloudDocs.Application.Features.DocumentTypes.ReactivateDocumentType;

public interface IReactivateDocumentTypeService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}