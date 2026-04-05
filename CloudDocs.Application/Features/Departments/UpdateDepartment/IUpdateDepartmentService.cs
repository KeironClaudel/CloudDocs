using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.UpdateDepartment;

public interface IUpdateDepartmentService
{
    Task<DepartmentResponse?> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
}