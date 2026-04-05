namespace CloudDocs.Application.Features.Departments.ReactivateDepartment;

public interface IReactivateDepartmentService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}