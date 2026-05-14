using ERPMultiTenant.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ERPMultiTenant.API.Authorization;

public sealed class PermissionRequirement(Permission permission) : IAuthorizationRequirement
{
    public Permission Permission { get; } = permission;
}
