namespace CloudDocs.Application.Features.DocumentTypes.DeactivateDocumentType;

public interface IDeactivateDocumentTypeService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}