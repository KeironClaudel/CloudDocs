using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.CreateDepartment;

public interface ICreateDepartmentService
{
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
}