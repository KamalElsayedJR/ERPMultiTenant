namespace ERPMultiTenant.Domain.Entities;

public sealed class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
