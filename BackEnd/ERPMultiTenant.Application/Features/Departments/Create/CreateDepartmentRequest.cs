using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.Create;

public sealed record CreateDepartmentRequest(string Name, string? Description) : IRequest<Result<CreateDepartmentResponse>>;
