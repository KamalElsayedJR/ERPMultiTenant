using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.API.Authorization;

public static class PermissionPolicyConstants
{
    public const string PolicyPrefix = "Permission:";

    public static string GetPolicyName(Permission permission) => $"{PolicyPrefix}{permission}";
}
