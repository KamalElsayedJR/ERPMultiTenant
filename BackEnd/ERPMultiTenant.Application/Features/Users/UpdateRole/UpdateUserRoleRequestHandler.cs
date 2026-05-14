using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Enums;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.UpdateRole;

public sealed class UpdateUserRoleRequestHandler(IUserRepository userRepository, ICurrentTenantService currentTenantService)
    : IRequestHandler<UpdateUserRoleRequest, Result<UpdateUserRoleResponse>>
{
    public async Task<Result<UpdateUserRoleResponse>> Handle(UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<UpdateUserRoleResponse>.Fail("Tenant context is missing.");
        }

        var user = await userRepository.GetByIdAsync(request.UserId, tenantId.Value, cancellationToken);
        if (user is null)
        {
            return Result<UpdateUserRoleResponse>.Fail("User not found.");
        }

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
        {
            var adminCount = await userRepository.CountByRoleAsync(tenantId.Value, UserRole.Admin, cancellationToken);
            if (adminCount <= 1)
            {
                return Result<UpdateUserRoleResponse>.Fail("Cannot downgrade the last admin in the tenant.");
            }
        }

        user.Role = request.Role;
        await userRepository.UpdateAsync(user, cancellationToken);

        var response = new UpdateUserRoleResponse(user.Id, user.Role);
        return Result<UpdateUserRoleResponse>.Ok(response);
    }
}
