using CloudDocs.Application.Features.Departments.Common;

namespace CloudDocs.Application.Features.Departments.GetDepartmentById;

public interface IGetDepartmentByIdService
{
    Task<DepartmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}