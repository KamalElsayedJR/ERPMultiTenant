using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.RefreshToken;

public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;
