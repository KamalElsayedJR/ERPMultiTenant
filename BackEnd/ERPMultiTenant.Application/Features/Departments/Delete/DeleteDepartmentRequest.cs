using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.Delete;

public sealed record DeleteDepartmentRequest(Guid DepartmentId) : IRequest<Result<DeleteDepartmentResponse>>;
