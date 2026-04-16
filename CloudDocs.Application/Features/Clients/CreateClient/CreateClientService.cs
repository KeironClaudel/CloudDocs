using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Clients.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Clients.CreateClient;

public class CreateClientService : ICreateClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateClientService(
        IClientRepository clientRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var nameExists = await _clientRepository.NameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new BadRequestException("Client name is already in use.");

        if (!string.IsNullOrWhiteSpace(request.Identification))
        {
            var identificationExists = await _clientRepository.IdentificationExistsAsync(request.Identification, cancellationToken);
            if (identificationExists)
                throw new BadRequestException("Client identification is already in use.");
        }

        var entity = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim(),
            Identification = string.IsNullOrWhiteSpace(request.Identification) ? null : request.Identification.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _clientRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Create",
            "Clients",
            "Client",
            entity.Id.ToString(),
            $"Client created: {entity.Name}",
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
            entity.CreatedAt);
    }
}