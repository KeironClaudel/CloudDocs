using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.GetClients;

public interface IGetClientsService
{
    Task<List<ClientResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}