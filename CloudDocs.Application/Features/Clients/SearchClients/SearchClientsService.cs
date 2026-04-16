using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.SearchClients;

public class SearchClientsService : ISearchClientsService
{
    private readonly IClientRepository _clientRepository;

    public SearchClientsService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<List<ClientResponse>> SearchByNameAsync(string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<ClientResponse>();

        var items = await _clientRepository.SearchByNameAsync(term, cancellationToken);

        return items.Select(x => new ClientResponse(
            x.Id,
            x.Name,
            x.LegalName,
            x.Identification,
            x.Email,
            x.Phone,
            x.Notes,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}