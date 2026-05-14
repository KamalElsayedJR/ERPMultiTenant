using ERPMultiTenant.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ERPMultiTenant.API.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission)
    {
        Policy = PermissionPolicyConstants.GetPolicyName(permission);
    }
}
