using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.List;

public sealed record GetEmployeesQuery(
    int? PageNumber,
    int? PageSize,
    string? SearchTerm,
    Guid? DepartmentId,
    decimal? MinSalary,
    decimal? MaxSalary,
    string? SortBy,
    string? SortDirection) : IRequest<Result<EmployeeListResponse>>;
