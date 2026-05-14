using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Entities;
using ERPMultiTenant.Domain.Enums;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Register;

public sealed class RegisterRequestHandler(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return Result<RegisterResponse>.Fail("Email already exists");
        }

        var companyName = request.CompanyName.Trim();
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return Result<RegisterResponse>.Fail("Company name is required.");
        }

        var slugBase = GenerateSlug(companyName);
        var slug = slugBase;
        var suffix = 1;
        while (await tenantRepository.SlugExistsAsync(slug, cancellationToken))
        {
            slug = $"{slugBase}-{suffix++}";
        }

        var tenant = new Tenant
        {
            Name = companyName,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        await tenantRepository.AddAsync(tenant, cancellationToken);

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = UserRole.Admin,
            TenantId = tenant.Id
        };

        await userRepository.AddAsync(user, cancellationToken);

        var response = new RegisterResponse(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);
        return Result<RegisterResponse>.Ok(response);
    }

    private static string GenerateSlug(string value)
    {
        var sanitized = new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        while (sanitized.Contains("--", StringComparison.Ordinal))
        {
            sanitized = sanitized.Replace("--", "-", StringComparison.Ordinal);
        }

        return sanitized.Trim('-');
    }
}
