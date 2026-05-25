using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Details;

public sealed record GetEmployeeDetailsQuery(Guid EmployeeId) : IRequest<Result<EmployeeDetailsResponse>>;
