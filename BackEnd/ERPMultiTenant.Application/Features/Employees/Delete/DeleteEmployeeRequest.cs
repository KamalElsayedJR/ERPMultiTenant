using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Delete;

public sealed record DeleteEmployeeRequest(Guid EmployeeId) : IRequest<Result<DeleteEmployeeResponse>>;
