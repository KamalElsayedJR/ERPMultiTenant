using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.Application.Features.Users.List;

public sealed record UserListItemResponse(Guid UserId, string FullName, string Email, UserRole Role, DateTime CreatedAt);
