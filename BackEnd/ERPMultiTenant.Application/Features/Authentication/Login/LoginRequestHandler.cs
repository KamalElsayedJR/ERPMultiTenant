using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Login;

public sealed class LoginRequestHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Fail("Invalid email or password.");
        }

        var tokenResult = tokenService.GenerateTokens(user);
        user.RefreshToken = tokenResult.RefreshToken;
        user.RefreshTokenExpiry = tokenResult.RefreshTokenExpiresAt;

        await userRepository.UpdateAsync(user, cancellationToken);

        var response = new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAt,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);

        return Result<LoginResponse>.Ok(response);
    }
}
