using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ERPMultiTenant.API.Authorization;

public sealed class PermissionHandler(ICurrentUserService currentUserService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (!currentUserService.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(currentUserService.Role))
        {
            return Task.CompletedTask;
        }

        if (!Enum.TryParse<UserRole>(currentUserService.Role, true, out var role))
        {
            return Task.CompletedTask;
        }

        var permissions = PermissionMappings.GetPermissions(role);
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
