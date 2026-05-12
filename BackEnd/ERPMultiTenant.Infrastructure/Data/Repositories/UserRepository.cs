using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Domain.Entities;
using ERPMultiTenant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPMultiTenant.Infrastructure.Data.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return context.ApplicationUsers.AnyAsync(user => user.Email == email, cancellationToken);
    }

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return context.ApplicationUsers.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.ApplicationUsers.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task AddAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await context.ApplicationUsers.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        context.ApplicationUsers.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
