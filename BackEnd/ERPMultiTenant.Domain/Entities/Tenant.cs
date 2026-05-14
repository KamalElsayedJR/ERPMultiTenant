namespace ERPMultiTenant.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    // TODO: Introduce soft-delete fields (e.g., IsDeleted/DeletedAt) when tenant removal is implemented.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
