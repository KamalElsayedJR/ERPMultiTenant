using System.Security.Claims;
using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.RefreshToken;

public sealed class RefreshTokenRequestHandler(IUserRepository userRepository, ITokenService tokenService)
    : IRequestHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return Result<RefreshTokenResponse>.Fail("Invalid access token.");
        }

        var userIdClaim = principal.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Result<RefreshTokenResponse>.Fail("Invalid access token.");
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<RefreshTokenResponse>.Fail("User not found.");
        }

        if (string.IsNullOrWhiteSpace(user.RefreshToken) ||
            user.RefreshTokenExpiry is null ||
            user.RefreshTokenExpiry <= DateTime.UtcNow ||
            !string.Equals(user.RefreshToken, request.RefreshToken, StringComparison.Ordinal))
        {
            return Result<RefreshTokenResponse>.Fail("Invalid refresh token.");
        }

        var tokenResult = tokenService.GenerateTokens(user);
        user.RefreshToken = tokenResult.RefreshToken;
        user.RefreshTokenExpiry = tokenResult.RefreshTokenExpiresAt;

        await userRepository.UpdateAsync(user, cancellationToken);

        var response = new RefreshTokenResponse(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAt,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);

        return Result<RefreshTokenResponse>.Ok(response);
    }
}
