using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.SearchClients;

public interface ISearchClientsService
{
    Task<List<ClientResponse>> SearchByNameAsync(string term, CancellationToken cancellationToken = default);
}