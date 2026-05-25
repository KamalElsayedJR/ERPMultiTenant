using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.List;

public sealed class GetEmployeesQueryHandler(
    IEmployeeRepository employeeRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetEmployeesQuery, Result<EmployeeListResponse>>
{
    public async Task<Result<EmployeeListResponse>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<EmployeeListResponse>.Fail("Tenant context is missing.");
        }

        var pageNumber = request.PageNumber.GetValueOrDefault(1);
        var pageSize = request.PageSize.GetValueOrDefault(25);
        var sortBy = request.SortBy?.Trim().ToLowerInvariant();
        var sortDirection = request.SortDirection?.Trim().ToLowerInvariant() ?? "asc";
        var sortDescending = sortDirection == "desc";
        var searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();

        var totalCount = await employeeRepository.CountAsync(
            tenantId.Value,
            searchTerm,
            request.DepartmentId,
            request.MinSalary,
            request.MaxSalary,
            cancellationToken);

        var employees = await employeeRepository.GetPagedAsync(
            tenantId.Value,
            pageNumber,
            pageSize,
            searchTerm,
            request.DepartmentId,
            request.MinSalary,
            request.MaxSalary,
            sortBy,
            sortDescending,
            cancellationToken);

        var items = employees
            .Select(employee => new EmployeeListItemResponse(
                employee.Id,
                employee.EmployeeNumber,
                employee.ApplicationUser?.FullName ?? string.Empty,
                employee.ApplicationUser?.Email ?? string.Empty,
                employee.DepartmentId,
                employee.Department?.Name ?? string.Empty,
                employee.JobTitle,
                employee.HireDate,
                employee.Salary))
            .ToList()
            .AsReadOnly();

        var response = new EmployeeListResponse(
            items,
            pageNumber,
            pageSize,
            totalCount,
            searchTerm,
            request.DepartmentId,
            request.MinSalary,
            request.MaxSalary,
            sortBy,
            sortDirection);

        return Result<EmployeeListResponse>.Ok(response);
    }
}
