using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.GetClientById;

public class GetClientByIdService : IGetClientByIdService
{
    private readonly IClientRepository _clientRepository;

    public GetClientByIdService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var x = await _clientRepository.GetByIdAsync(id, cancellationToken);

        if (x is null)
            return null;

        return new ClientResponse(
            x.Id,
            x.Name,
            x.LegalName,
            x.Identification,
            x.Email,
            x.Phone,
            x.Notes,
            x.IsActive,
            x.CreatedAt);
    }
}