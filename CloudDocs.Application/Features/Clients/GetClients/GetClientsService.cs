using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.GetClients;

public class GetClientsService : IGetClientsService
{
    private readonly IClientRepository _clientRepository;

    public GetClientsService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<List<ClientResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _clientRepository.GetAllAsync(cancellationToken);

        return items.Select(x => new ClientResponse(
            x.Id,
            x.Name,
            x.LegalName,
            x.Identification,
            x.Email,
            x.Phone,
            x.Notes,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt)).ToList();
    }
}
