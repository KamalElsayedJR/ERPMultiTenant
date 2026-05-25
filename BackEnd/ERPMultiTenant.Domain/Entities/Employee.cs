namespace ERPMultiTenant.Domain.Entities;

public sealed class Employee : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public DateOnly HireDate { get; set; }
    public decimal Salary { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}
