using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Departments.ReactivateDepartment;

public class ReactivateDepartmentService : IReactivateDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateDepartmentService(
        IDepartmentRepository departmentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = true;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _departmentRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Reactivate",
            "Departments",
            "Department",
            entity.Id.ToString(),
            $"Department reactivated: {entity.Name}",
            null,
            cancellationToken);

        return true;
    }
}