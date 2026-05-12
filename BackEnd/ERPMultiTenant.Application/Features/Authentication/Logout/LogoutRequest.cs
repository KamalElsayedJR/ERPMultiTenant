using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Authentication.Logout;

public sealed record LogoutRequest : IRequest<Result<string>>;
