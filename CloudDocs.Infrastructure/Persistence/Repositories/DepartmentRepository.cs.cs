using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Department>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var departmentIds = ids.Distinct().ToList();

        return await _context.Departments
            .Where(x => departmentIds.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}