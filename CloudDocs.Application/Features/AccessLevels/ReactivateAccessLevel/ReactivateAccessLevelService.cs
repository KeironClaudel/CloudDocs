using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.AccessLevels.ReactivateAccessLevel;

public class ReactivateAccessLevelService : IReactivateAccessLevelService
{
    private readonly IAccessLevelRepository _accessLevelRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateAccessLevelService(
        IAccessLevelRepository accessLevelRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _accessLevelRepository = accessLevelRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _accessLevelRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = true;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _accessLevelRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Reactivate",
            "AccessLevels",
            "AccessLevel",
            entity.Id.ToString(),
            $"Access level reactivated: {entity.Name} ({entity.Code})",
            null,
            cancellationToken);

        return true;
    }
}
