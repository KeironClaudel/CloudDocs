using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.GetDepartmentById;

public class GetDepartmentByIdService : IGetDepartmentByIdService
{
    private readonly IDepartmentRepository _departmentRepository;

    public GetDepartmentByIdService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<DepartmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var x = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (x is null)
            return null;

        return new DepartmentResponse(
            x.Id,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt);
    }
}