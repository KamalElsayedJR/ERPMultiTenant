using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Logout;

public sealed class LogoutRequestHandler(ICurrentUserService currentUserService, IUserRepository userRepository)
    : IRequestHandler<LogoutRequest, Result<string>>
{
    public async Task<Result<string>> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            return Result<string>.Fail("Unauthorized");
        }

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Result<string>.Fail("User not found.");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<string>.Ok("Logged out successfully.");
    }
}
