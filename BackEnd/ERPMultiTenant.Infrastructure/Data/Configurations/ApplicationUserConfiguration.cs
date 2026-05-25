using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPMultiTenant.Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(user => user.Id);

        builder.HasAlternateKey(user => new { user.TenantId, user.Id });

        builder.Property(user => user.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.TenantId)
            .IsRequired();

        builder.Property(user => user.RefreshToken)
            .HasMaxLength(512);

        builder.Property(user => user.RefreshTokenExpiry);

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.HasOne(user => user.Employee)
            .WithOne(employee => employee.ApplicationUser)
            .HasForeignKey<Employee>(employee => new { employee.TenantId, employee.ApplicationUserId })
            .HasPrincipalKey<ApplicationUser>(user => new { user.TenantId, user.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
