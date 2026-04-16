using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Clients.DeactivateClient;

public class DeactivateClientService : IDeactivateClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateClientService(
        IClientRepository clientRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Deactivate",
            "Clients",
            "Client",
            entity.Id.ToString(),
            $"Client deactivated: {entity.Name}",
            null,
            cancellationToken);

        return true;
    }
}