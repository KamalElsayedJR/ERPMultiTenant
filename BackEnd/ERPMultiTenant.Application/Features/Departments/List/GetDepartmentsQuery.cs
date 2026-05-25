using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.List;

public sealed record GetDepartmentsQuery(int? PageNumber, int? PageSize) : IRequest<Result<DepartmentListResponse>>;
