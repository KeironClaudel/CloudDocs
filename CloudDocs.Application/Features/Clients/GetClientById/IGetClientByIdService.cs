using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.GetClientById;

public interface IGetClientByIdService
{
    Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}