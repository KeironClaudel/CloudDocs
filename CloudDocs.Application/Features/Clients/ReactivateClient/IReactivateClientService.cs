namespace CloudDocs.Application.Features.Clients.ReactivateClient;

public interface IReactivateClientService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}