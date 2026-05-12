using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.Profile;

public sealed class UserProfileQueryHandler(IUserRepository userRepository)
    : IRequestHandler<UserProfileQuery, Result<UserProfileResponse>>
{
    public async Task<Result<UserProfileResponse>> Handle(UserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<UserProfileResponse>.Fail("User not found.");
        }

        var response = new UserProfileResponse(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);
        return Result<UserProfileResponse>.Ok(response);
    }
}
