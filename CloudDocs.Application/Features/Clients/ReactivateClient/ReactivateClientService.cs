using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Clients.ReactivateClient;

public class ReactivateClientService : IReactivateClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateClientService(
        IClientRepository clientRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = true;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Reactivate",
            "Clients",
            "Client",
            entity.Id.ToString(),
            $"Client reactivated: {entity.Name}",
            null,
            cancellationToken);

        return true;
    }
}