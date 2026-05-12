using Microsoft.EntityFrameworkCore;

namespace ERPMultiTenant.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options);
