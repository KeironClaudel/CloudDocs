using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.AccessLevels.DeactivateAccessLevel;

public class DeactivateAccessLevelService : IDeactivateAccessLevelService
{
    private readonly IAccessLevelRepository _accessLevelRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAccessLevelService(
        IAccessLevelRepository accessLevelRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _accessLevelRepository = accessLevelRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _accessLevelRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _accessLevelRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Deactivate",
            "AccessLevels",
            "AccessLevel",
            entity.Id.ToString(),
            $"Access level deactivated: {entity.Name} ({entity.Code})",
            null,
            cancellationToken);

        return true;
    }
}
