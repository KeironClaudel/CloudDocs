using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.GetDepartments;

public interface IGetDepartmentsService
{
    Task<List<DepartmentResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}