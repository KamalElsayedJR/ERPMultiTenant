using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.Profile;

public sealed record UserProfileQuery(Guid UserId, string Email, string Role, Guid TenantId) : IRequest<Result<UserProfileResponse>>;
