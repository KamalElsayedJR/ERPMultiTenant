using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Enums;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.UpdateRole;

public sealed record UpdateUserRoleRequest(Guid UserId, UserRole Role) : IRequest<Result<UpdateUserRoleResponse>>;
