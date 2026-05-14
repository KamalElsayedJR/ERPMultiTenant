using ERPMultiTenant.Domain.Entities;
using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApplicationUser>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<int> CountByRoleAsync(Guid tenantId, UserRole role, CancellationToken cancellationToken);
    Task AddAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task DeleteAsync(ApplicationUser user, CancellationToken cancellationToken);
}
