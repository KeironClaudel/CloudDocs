using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.UpdateClient;

public interface IUpdateClientService
{
    Task<ClientResponse?> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default);
}