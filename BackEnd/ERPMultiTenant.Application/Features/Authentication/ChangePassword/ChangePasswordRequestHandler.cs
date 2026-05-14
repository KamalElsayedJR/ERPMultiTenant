using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.ChangePassword;

public sealed class ChangePasswordRequestHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ChangePasswordRequest, Result<ChangePasswordResponse>>
{
    public async Task<Result<ChangePasswordResponse>> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            return Result<ChangePasswordResponse>.Fail("Unauthorized");
        }

        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<ChangePasswordResponse>.Fail("Tenant context is missing.");
        }

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value, tenantId.Value, cancellationToken);
        if (user is null)
        {
            return Result<ChangePasswordResponse>.Fail("User not found.");
        }

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return Result<ChangePasswordResponse>.Fail("Current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        await userRepository.UpdateAsync(user, cancellationToken);

        var response = new ChangePasswordResponse(user.Id);
        return Result<ChangePasswordResponse>.Ok(response);
    }
}
