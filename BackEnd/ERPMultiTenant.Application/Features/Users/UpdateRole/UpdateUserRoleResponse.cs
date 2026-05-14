using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.Application.Features.Users.UpdateRole;

public sealed record UpdateUserRoleResponse(Guid UserId, UserRole Role);
