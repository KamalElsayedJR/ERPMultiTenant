using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.API.Authorization;

public static class PermissionMappings
{
    private static readonly IReadOnlyDictionary<UserRole, Permission[]> RolePermissions =
        new Dictionary<UserRole, Permission[]>
        {
            [UserRole.Admin] = Enum.GetValues<Permission>(),
            [UserRole.Manager] = new[]
            {
                Permission.ManageExpenses,
                Permission.ViewDashboard,
                Permission.ManageInvoices,
                Permission.EmployeesView,
                Permission.EmployeesCreate,
                Permission.EmployeesUpdate,
                Permission.DepartmentsView,
                Permission.DepartmentsCreate,
                Permission.DepartmentsUpdate
            },
            [UserRole.Employee] = new[]
            {
                Permission.ViewDashboard,
                Permission.EmployeesView,
                Permission.DepartmentsView
            },
            [UserRole.Accountant] = new[]
            {
                Permission.ManageInvoices,
                Permission.ViewDashboard,
                Permission.EmployeesView,
                Permission.DepartmentsView
            }
        };

    public static IReadOnlyCollection<Permission> GetPermissions(UserRole role)
    {
        return RolePermissions.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<Permission>();
    }
}
