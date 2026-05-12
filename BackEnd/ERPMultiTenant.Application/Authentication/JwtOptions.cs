namespace ERPMultiTenant.Application.Authentication;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 60;
    public int RefreshTokenDays { get; init; } = 7;
}
