using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.CreateClient;

public interface ICreateClientService
{
    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
}