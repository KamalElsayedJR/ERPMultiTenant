namespace ERPMultiTenant.Application.Models;

public sealed class TokenResult
{
    public TokenResult(string accessToken, DateTime accessTokenExpiresAt, string refreshToken, DateTime refreshTokenExpiresAt)
    {
        AccessToken = accessToken;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
    }

    public string AccessToken { get; }
    public DateTime AccessTokenExpiresAt { get; }
    public string RefreshToken { get; }
    public DateTime RefreshTokenExpiresAt { get; }
}
