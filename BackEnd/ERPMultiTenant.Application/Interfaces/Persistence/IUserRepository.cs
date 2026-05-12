using ERPMultiTenant.Domain.Entities;

namespace ERPMultiTenant.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
}
