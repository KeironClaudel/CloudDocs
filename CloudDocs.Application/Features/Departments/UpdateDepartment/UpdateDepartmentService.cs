using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.UpdateDepartment;

public class UpdateDepartmentService : IUpdateDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentService(
        IDepartmentRepository departmentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentResponse?> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        var exists = await _departmentRepository.NameExistsAsync(request.Name, cancellationToken);
        var normalizedCurrent = entity.Name.Trim().ToLower();
        var normalizedRequested = request.Name.Trim().ToLower();

        if (exists && normalizedCurrent != normalizedRequested)
            throw new BadRequestException("Department name is already in use.");

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _departmentRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Update",
            "Departments",
            "Department",
            entity.Id.ToString(),
            $"Department updated: {entity.Name}",
            null,
            cancellationToken);

        return new DepartmentResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);
    }
}