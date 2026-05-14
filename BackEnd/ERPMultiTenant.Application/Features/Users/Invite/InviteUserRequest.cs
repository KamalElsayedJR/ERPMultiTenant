using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Enums;
using MediatR;

namespace ERPMultiTenant.Application.Features.Users.Invite;

public sealed record InviteUserRequest(string FullName, string Email, UserRole Role) : IRequest<Result<InviteUserResponse>>;
