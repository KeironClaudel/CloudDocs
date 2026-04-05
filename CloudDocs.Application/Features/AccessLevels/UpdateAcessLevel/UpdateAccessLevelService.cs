using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.AccessLevels.Common;

namespace CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;

public class UpdateAccessLevelService : IUpdateAccessLevelService
{
    private readonly IAccessLevelRepository _accessLevelRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccessLevelService(
        IAccessLevelRepository accessLevelRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _accessLevelRepository = accessLevelRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccessLevelResponse?> UpdateAsync(Guid id, UpdateAccessLevelRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _accessLevelRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        var exists = await _accessLevelRepository.NameExistsAsync(request.Name, cancellationToken);
        var normalizedCurrent = entity.Name.Trim().ToLower();
        var normalizedRequested = request.Name.Trim().ToLower();

        if (exists && normalizedCurrent != normalizedRequested)
            throw new BadRequestException("Access level name is already in use.");

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _accessLevelRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Update",
            "AccessLevels",
            "AccessLevel",
            entity.Id.ToString(),
            $"Access level updated: {entity.Name} ({entity.Code})",
            null,
            cancellationToken);

        return new AccessLevelResponse(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);
    }
}