using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.List;

public sealed class GetUsersQueryHandler(IUserRepository userRepository, ICurrentTenantService currentTenantService)
    : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserListItemResponse>>>
{
    public async Task<Result<IReadOnlyList<UserListItemResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<IReadOnlyList<UserListItemResponse>>.Fail("Tenant context is missing.");
        }

        var users = await userRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);
        var response = users
            .Select(user => new UserListItemResponse(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<UserListItemResponse>>.Ok(response);
    }
}
