using ERPMultiTenant.Application.Interfaces.Authentication;
using System.Security.Cryptography;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Entities;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.Invite;

public sealed class InviteUserRequestHandler(
    IUserRepository userRepository,
    ICurrentTenantService currentTenantService,
    IPasswordHasher passwordHasher)
    : IRequestHandler<InviteUserRequest, Result<InviteUserResponse>>
{
    public async Task<Result<InviteUserResponse>> Handle(InviteUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<InviteUserResponse>.Fail("Tenant context is missing.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return Result<InviteUserResponse>.Fail("Email already exists");
        }

        var temporaryPassword = GenerateTemporaryPassword();
        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(temporaryPassword),
            Role = request.Role,
            TenantId = tenantId.Value
        };

        await userRepository.AddAsync(user, cancellationToken);

        // TODO: Replace temporary password return with an email invitation workflow.
        // TODO: Introduce invite tokens to support secure invitation acceptance.
        // TODO: Add a set-password flow for invited users.
        var response = new InviteUserResponse(user.Email, user.Role, temporaryPassword);
        return Result<InviteUserResponse>.Ok(response);
    }

    private static string GenerateTemporaryPassword()
    {
        const int length = 12;
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string symbols = "@$!%*?&";
        var all = string.Concat(lower, upper, digits, symbols);

        var chars = new char[length];
        chars[0] = lower[System.Security.Cryptography.RandomNumberGenerator.GetInt32(lower.Length)];
        chars[1] = upper[System.Security.Cryptography.RandomNumberGenerator.GetInt32(upper.Length)];
        chars[2] = digits[System.Security.Cryptography.RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = symbols[System.Security.Cryptography.RandomNumberGenerator.GetInt32(symbols.Length)];

        for (var i = 4; i < length; i++)
        {
            chars[i] = all[System.Security.Cryptography.RandomNumberGenerator.GetInt32(all.Length)];
        }

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var swapIndex = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[swapIndex]) = (chars[swapIndex], chars[i]);
        }

        return new string(chars);
    }
}
