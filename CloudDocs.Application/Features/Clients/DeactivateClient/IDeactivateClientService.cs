namespace CloudDocs.Application.Features.Clients.DeactivateClient;

public interface IDeactivateClientService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}