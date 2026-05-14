using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.Domain.Entities;

public sealed class ApplicationUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
