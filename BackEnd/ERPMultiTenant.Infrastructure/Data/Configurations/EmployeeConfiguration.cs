using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPMultiTenant.Infrastructure.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Employees_Salary_NonNegative", "[Salary] >= 0");
        });

        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.EmployeeNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(employee => employee.JobTitle)
            .HasMaxLength(150);

        builder.Property(employee => employee.HireDate)
            .IsRequired();

        builder.Property(employee => employee.Salary)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(employee => employee.TenantId)
            .IsRequired();

        builder.Property(employee => employee.DepartmentId)
            .IsRequired();

        builder.Property(employee => employee.ApplicationUserId)
            .IsRequired();

        builder.Property(employee => employee.CreatedAt)
            .IsRequired();

        builder.HasIndex(employee => new { employee.TenantId, employee.EmployeeNumber })
            .IsUnique();

        builder.HasIndex(employee => employee.TenantId);

        builder.HasOne(employee => employee.Department)
            .WithMany(department => department.Employees)
            .HasForeignKey(employee => new { employee.TenantId, employee.DepartmentId })
            .HasPrincipalKey(department => new { department.TenantId, department.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(employee => employee.ApplicationUser)
            .WithOne(user => user.Employee)
            .HasForeignKey<Employee>(employee => new { employee.TenantId, employee.ApplicationUserId })
            .HasPrincipalKey<ApplicationUser>(user => new { user.TenantId, user.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
