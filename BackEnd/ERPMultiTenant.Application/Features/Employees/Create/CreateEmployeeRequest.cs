using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Create;

public sealed record CreateEmployeeRequest(
    string? JobTitle,
    DateOnly HireDate,
    decimal Salary,
    Guid DepartmentId,
    Guid ApplicationUserId) : IRequest<Result<CreateEmployeeResponse>>;
