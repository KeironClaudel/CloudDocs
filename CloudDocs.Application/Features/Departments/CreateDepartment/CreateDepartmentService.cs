using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Departments.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Departments.CreateDepartment;

public class CreateDepartmentService : ICreateDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentService(
        IDepartmentRepository departmentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _departmentRepository.NameExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new BadRequestException("Department name is already in use.");

        var entity = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _departmentRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Create",
            "Departments",
            "Department",
            entity.Id.ToString(),
            $"Department created: {entity.Name}",
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