using ERPMultiTenant.Domain.Entities;

namespace ERPMultiTenant.Application.Interfaces.Persistence;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
}
