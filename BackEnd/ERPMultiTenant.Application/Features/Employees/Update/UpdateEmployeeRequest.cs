using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Update;

public sealed record UpdateEmployeeRequest(
    Guid EmployeeId,
    string EmployeeNumber,
    string? JobTitle,
    DateOnly HireDate,
    decimal Salary,
    Guid DepartmentId,
    Guid ApplicationUserId) : IRequest<Result<UpdateEmployeeResponse>>;
