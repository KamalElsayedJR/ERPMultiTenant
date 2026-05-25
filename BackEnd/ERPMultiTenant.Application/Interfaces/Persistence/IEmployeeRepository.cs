using ERPMultiTenant.Domain.Entities;

namespace ERPMultiTenant.Application.Interfaces.Persistence;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetPagedAsync(
        Guid tenantId,
        int pageNumber,
        int pageSize,
        string? searchTerm,
        Guid? departmentId,
        decimal? minSalary,
        decimal? maxSalary,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        Guid tenantId,
        string? searchTerm,
        Guid? departmentId,
        decimal? minSalary,
        decimal? maxSalary,
        CancellationToken cancellationToken);

    Task<Employee?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken);
    Task<bool> EmployeeNumberExistsAsync(Guid tenantId, string employeeNumber, Guid? excludeEmployeeId, CancellationToken cancellationToken);
    Task<bool> ApplicationUserAssignedAsync(Guid tenantId, Guid applicationUserId, Guid? excludeEmployeeId, CancellationToken cancellationToken);
    Task<int> GetNextEmployeeNumberSequenceAsync(Guid tenantId, Guid departmentId, CancellationToken cancellationToken);
    Task AddAsync(Employee employee, CancellationToken cancellationToken);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken);
    Task DeleteAsync(Employee employee, CancellationToken cancellationToken);
}
