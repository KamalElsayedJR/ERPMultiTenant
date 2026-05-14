using ERPMultiTenant.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace ERPMultiTenant.Infrastructure.Authentication;

public sealed class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenantService
{
    public Guid? CurrentTenantId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value;
            return Guid.TryParse(value, out var tenantId) ? tenantId : null;
        }
    }
}
