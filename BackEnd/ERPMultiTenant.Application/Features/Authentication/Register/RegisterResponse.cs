using ERPMultiTenant.Domain.Enums;

namespace ERPMultiTenant.Application.Features.Authentication.Register;

public sealed class RegisterResponse
{
    public RegisterResponse(Guid userId, string fullName, string email, UserRole role, DateTime createdAt)
    {
        UserId = userId;
        FullName = fullName;
        Email = email;
        Role = role;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; }
    public string FullName { get; }
    public string Email { get; }
    public UserRole Role { get; }
    public DateTime CreatedAt { get; }
}
