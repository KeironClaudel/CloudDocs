using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Clients.Common;

namespace CloudDocs.Application.Features.Clients.UpdateClient;

public class UpdateClientService : IUpdateClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClientService(
        IClientRepository clientRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientResponse?> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        var normalizedCurrentName = entity.Name.Trim().ToLower();
        var normalizedRequestedName = request.Name.Trim().ToLower();

        var nameExists = await _clientRepository.NameExistsAsync(request.Name, cancellationToken);
        if (nameExists && normalizedCurrentName != normalizedRequestedName)
            throw new BadRequestException("Client name is already in use.");

        if (!string.IsNullOrWhiteSpace(request.Identification))
        {
            var normalizedCurrentIdentification = entity.Identification?.Trim().ToLower();
            var normalizedRequestedIdentification = request.Identification.Trim().ToLower();

            var identificationExists = await _clientRepository.IdentificationExistsAsync(request.Identification, cancellationToken);
            if (identificationExists && normalizedCurrentIdentification != normalizedRequestedIdentification)
                throw new BadRequestException("Client identification is already in use.");
        }

        entity.Name = request.Name.Trim();
        entity.LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim();
        entity.Identification = string.IsNullOrWhiteSpace(request.Identification) ? null : request.Identification.Trim();
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Update",
            "Clients",
            "Client",
            entity.Id.ToString(),
            $"Client updated: {entity.Name}",
            null,
            cancellationToken);

        return new ClientResponse(
            entity.Id,
            entity.Name,
            entity.LegalName,
            entity.Identification,
            entity.Email,
            entity.Phone,
            entity.Notes,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
