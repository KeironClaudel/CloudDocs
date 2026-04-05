using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.GetDepartments;

public class GetDepartmentsService : IGetDepartmentsService
{
    private readonly IDepartmentRepository _departmentRepository;

    public GetDepartmentsService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<List<DepartmentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _departmentRepository.GetAllAsync(cancellationToken);

        return items.Select(x => new DepartmentResponse(
            x.Id,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}