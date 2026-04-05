namespace CloudDocs.Application.Features.Departments.DeactivateDepartment;

public interface IDeactivateDepartmentService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}