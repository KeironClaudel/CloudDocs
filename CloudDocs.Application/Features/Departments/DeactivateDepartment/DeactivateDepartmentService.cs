using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Departments.DeactivateDepartment;

public class DeactivateDepartmentService : IDeactivateDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDepartmentService(
        IDepartmentRepository departmentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _departmentRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Deactivate",
            "Departments",
            "Department",
            entity.Id.ToString(),
            $"Department deactivated: {entity.Name}",
            null,
            cancellationToken);

        return true;
    }
}