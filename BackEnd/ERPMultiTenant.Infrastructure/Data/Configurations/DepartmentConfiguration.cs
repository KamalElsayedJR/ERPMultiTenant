using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPMultiTenant.Infrastructure.Data.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(department => department.Id);

        builder.HasAlternateKey(department => new { department.TenantId, department.Id });

        builder.Property(department => department.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(department => department.Description)
            .HasMaxLength(500);

        builder.Property(department => department.TenantId)
            .IsRequired();

        builder.HasIndex(department => new { department.TenantId, department.Name })
            .IsUnique();

        builder.HasIndex(department => department.TenantId);

        builder.Property(department => department.CreatedAt)
            .IsRequired();

        builder.HasMany(department => department.Employees)
            .WithOne(employee => employee.Department)
            .HasForeignKey(employee => new { employee.TenantId, employee.DepartmentId })
            .HasPrincipalKey(department => new { department.TenantId, department.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
