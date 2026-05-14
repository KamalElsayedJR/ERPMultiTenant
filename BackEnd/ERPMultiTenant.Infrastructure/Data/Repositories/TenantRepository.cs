using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERPMultiTenant.Infrastructure.Data.Repositories;

public sealed class TenantRepository(ApplicationDbContext context) : ITenantRepository
{
    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        await context.Tenants.AddAsync(tenant, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
    {
        // TODO: Exclude soft-deleted tenants once soft-delete is implemented.
        return context.Tenants.AnyAsync(tenant => tenant.Slug == slug, cancellationToken);
    }
}
