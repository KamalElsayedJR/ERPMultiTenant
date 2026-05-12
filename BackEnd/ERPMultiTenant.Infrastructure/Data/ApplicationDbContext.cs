using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPMultiTenant.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    //public DbSet<ApplicationUser> Users { get; set; }
}
