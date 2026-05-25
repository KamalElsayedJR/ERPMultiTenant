using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.Update;

public sealed record UpdateDepartmentRequest(Guid DepartmentId, string Name, string? Description)
    : IRequest<Result<UpdateDepartmentResponse>>;
