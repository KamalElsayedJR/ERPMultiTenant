using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPMultiTenant.Infrastructure.Data.Repositories;

public sealed class EmployeeRepository(ApplicationDbContext context) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetPagedAsync(
        Guid tenantId,
        int pageNumber,
        int pageSize,
        string? searchTerm,
        Guid? departmentId,
        decimal? minSalary,
        decimal? maxSalary,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(tenantId, searchTerm, departmentId, minSalary, maxSalary)
            .Include(employee => employee.ApplicationUser)
            .Include(employee => employee.Department);

        var sortedQuery = ApplySorting(query, sortBy, sortDescending);

        return await sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid tenantId,
        string? searchTerm,
        Guid? departmentId,
        decimal? minSalary,
        decimal? maxSalary,
        CancellationToken cancellationToken)
    {
        return BuildFilteredQuery(tenantId, searchTerm, departmentId, minSalary, maxSalary)
            .CountAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
    {
        return context.Employees
            .Include(employee => employee.ApplicationUser)
            .Include(employee => employee.Department)
            .FirstOrDefaultAsync(employee => employee.Id == id && employee.TenantId == tenantId, cancellationToken);
    }

    public Task<bool> EmployeeNumberExistsAsync(Guid tenantId, string employeeNumber, Guid? excludeEmployeeId, CancellationToken cancellationToken)
    {
        return context.Employees.AnyAsync(
            employee => employee.TenantId == tenantId
                        && employee.EmployeeNumber == employeeNumber
                        && (!excludeEmployeeId.HasValue || employee.Id != excludeEmployeeId.Value),
            cancellationToken);
    }

    public Task<bool> ApplicationUserAssignedAsync(Guid tenantId, Guid applicationUserId, Guid? excludeEmployeeId, CancellationToken cancellationToken)
    {
        return context.Employees.AnyAsync(
            employee => employee.TenantId == tenantId
                        && employee.ApplicationUserId == applicationUserId
                        && (!excludeEmployeeId.HasValue || employee.Id != excludeEmployeeId.Value),
            cancellationToken);
    }

    public async Task<int> GetNextEmployeeNumberSequenceAsync(Guid tenantId, Guid departmentId, CancellationToken cancellationToken)
    {
        var lastEmployeeNumber = await context.Employees
            .Where(employee => employee.TenantId == tenantId && employee.DepartmentId == departmentId)
            .OrderByDescending(employee => employee.EmployeeNumber)
            .Select(employee => employee.EmployeeNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(lastEmployeeNumber))
        {
            return 1;
        }

        var separatorIndex = lastEmployeeNumber.LastIndexOf('-');
        if (separatorIndex < 0 || separatorIndex == lastEmployeeNumber.Length - 1)
        {
            return 1;
        }

        var sequencePart = lastEmployeeNumber[(separatorIndex + 1)..];
        return int.TryParse(sequencePart, out var sequence)
            ? sequence + 1
            : 1;
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        await context.Employees.AddAsync(employee, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken)
    {
        context.Employees.Update(employee);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken)
    {
        context.Employees.Remove(employee);
        await context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Employee> BuildFilteredQuery(
        Guid tenantId,
        string? searchTerm,
        Guid? departmentId,
        decimal? minSalary,
        decimal? maxSalary)
    {
        var query = context.Employees
            .Where(employee => employee.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(employee =>
                employee.EmployeeNumber.ToLower()!.Contains(normalized)
                || (employee.ApplicationUser != null && employee.ApplicationUser.FullName.ToLower()!.Contains(normalized))
                || (employee.ApplicationUser != null && employee.ApplicationUser.Email.ToLower()!.Contains(normalized)));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(employee => employee.DepartmentId == departmentId.Value);
        }

        if (minSalary.HasValue)
        {
            query = query.Where(employee => employee.Salary >= minSalary.Value);
        }

        if (maxSalary.HasValue)
        {
            query = query.Where(employee => employee.Salary <= maxSalary.Value);
        }

        return query;
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, bool sortDescending)
    {
        var normalized = sortBy?.Trim().ToLowerInvariant();
        return (normalized, sortDescending) switch
        {
            ("name", false) => query.OrderBy(employee => employee.ApplicationUser!.FullName),
            ("name", true) => query.OrderByDescending(employee => employee.ApplicationUser!.FullName),
            ("hiredate", false) => query.OrderBy(employee => employee.HireDate),
            ("hiredate", true) => query.OrderByDescending(employee => employee.HireDate),
            ("salary", false) => query.OrderBy(employee => employee.Salary),
            ("salary", true) => query.OrderByDescending(employee => employee.Salary),
            ("department", false) => query.OrderBy(employee => employee.Department!.Name),
            ("department", true) => query.OrderByDescending(employee => employee.Department!.Name),
            (_, false) => query.OrderBy(employee => employee.EmployeeNumber),
            (_, true) => query.OrderByDescending(employee => employee.EmployeeNumber)
        };
    }
}
