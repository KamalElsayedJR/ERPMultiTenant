using System.Security.Claims;
using ERPMultiTenant.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace ERPMultiTenant.Infrastructure.Authentication;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email => httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value;

    public string? Role => httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
