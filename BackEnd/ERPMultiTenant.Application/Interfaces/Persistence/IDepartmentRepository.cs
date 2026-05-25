using ERPMultiTenant.Domain.Entities;

namespace ERPMultiTenant.Application.Interfaces.Persistence;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetPagedByTenantIdAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<int> CountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Department?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeDepartmentId, CancellationToken cancellationToken);
    Task<bool> HasEmployeesAsync(Guid departmentId, Guid tenantId, CancellationToken cancellationToken);
    Task AddAsync(Department department, CancellationToken cancellationToken);
    Task UpdateAsync(Department department, CancellationToken cancellationToken);
    Task DeleteAsync(Department department, CancellationToken cancellationToken);
}
