using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Enums;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.Delete;

public sealed class DeleteUserRequestHandler(IUserRepository userRepository, ICurrentTenantService currentTenantService)
    : IRequestHandler<DeleteUserRequest, Result<DeleteUserResponse>>
{
    public async Task<Result<DeleteUserResponse>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<DeleteUserResponse>.Fail("Tenant context is missing.");
        }

        var user = await userRepository.GetByIdAsync(request.UserId, tenantId.Value, cancellationToken);
        if (user is null)
        {
            return Result<DeleteUserResponse>.Fail("User not found.");
        }

        if (user.Role == UserRole.Admin)
        {
            var adminCount = await userRepository.CountByRoleAsync(tenantId.Value, UserRole.Admin, cancellationToken);
            if (adminCount <= 1)
            {
                return Result<DeleteUserResponse>.Fail("Cannot delete the last admin in the tenant.");
            }
        }

        await userRepository.DeleteAsync(user, cancellationToken);

        var response = new DeleteUserResponse(user.Id);
        return Result<DeleteUserResponse>.Ok(response);
    }
}
