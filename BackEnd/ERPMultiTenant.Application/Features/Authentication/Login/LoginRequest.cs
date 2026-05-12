using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Login;

public sealed record LoginRequest(string Email, string Password) : IRequest<Result<LoginResponse>>;
