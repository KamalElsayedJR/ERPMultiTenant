using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Entities;
using ERPMultiTenant.Domain.Enums;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Register;

public sealed class RegisterRequestHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result<RegisterResponse>.Fail("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = UserRole.Employee
        };

        await userRepository.AddAsync(user, cancellationToken);

        var response = new RegisterResponse(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);
        return Result<RegisterResponse>.Ok(response);
    }
}
