using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Entities;
using System.Security.Claims;

namespace ERPMultiTenant.Application.Interfaces.Authentication;

public interface ITokenService
{
    TokenResult GenerateTokens(ApplicationUser user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
