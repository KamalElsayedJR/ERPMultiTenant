using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.API.Authorization;

public static class PermissionMappings
{
    private static readonly IReadOnlyDictionary<UserRole, Permission[]> RolePermissions =
        new Dictionary<UserRole, Permission[]>
        {
            [UserRole.Admin] = Enum.GetValues<Permission>(),
            [UserRole.Manager] = new[] { Permission.ManageExpenses, Permission.ViewDashboard, Permission.ManageInvoices },
            [UserRole.Employee] = new[] { Permission.ViewDashboard },
            [UserRole.Accountant] = new[] { Permission.ManageInvoices, Permission.ViewDashboard }
        };

    public static IReadOnlyCollection<Permission> GetPermissions(UserRole role)
    {
        return RolePermissions.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<Permission>();
    }
}
