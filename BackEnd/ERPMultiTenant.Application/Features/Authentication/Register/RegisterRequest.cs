using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Register;

public sealed record RegisterRequest(string FullName, string Email, string Password) : IRequest<Result<RegisterResponse>>;
