using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPMultiTenant.Infrastructure.Data.Repositories;

public sealed class DepartmentRepository(ApplicationDbContext context) : IDepartmentRepository
{
    public async Task<IReadOnlyList<Department>> GetPagedByTenantIdAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await context.Departments
            .Where(department => department.TenantId == tenantId)
            .OrderBy(department => department.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return context.Departments.CountAsync(department => department.TenantId == tenantId, cancellationToken);
    }

    public Task<Department?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
    {
        return context.Departments.FirstOrDefaultAsync(
            department => department.Id == id && department.TenantId == tenantId,
            cancellationToken);
    }

    public Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeDepartmentId, CancellationToken cancellationToken)
    {
        return context.Departments.AnyAsync(
            department => department.TenantId == tenantId
                          && department.Name == name
                          && (!excludeDepartmentId.HasValue || department.Id != excludeDepartmentId.Value),
            cancellationToken);
    }

    public Task<bool> HasEmployeesAsync(Guid departmentId, Guid tenantId, CancellationToken cancellationToken)
    {
        return context.Employees.AnyAsync(
            employee => employee.DepartmentId == departmentId && employee.TenantId == tenantId,
            cancellationToken);
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken)
    {
        await context.Departments.AddAsync(department, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Department department, CancellationToken cancellationToken)
    {
        context.Departments.Update(department);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Department department, CancellationToken cancellationToken)
    {
        context.Departments.Remove(department);
        await context.SaveChangesAsync(cancellationToken);
    }
}
