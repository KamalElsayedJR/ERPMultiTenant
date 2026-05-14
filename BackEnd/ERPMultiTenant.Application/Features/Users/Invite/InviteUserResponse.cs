using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.Application.Features.Users.Invite;

public sealed record InviteUserResponse(string Email, UserRole Role, string TemporaryPassword);
