using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Health;

public sealed record HealthQuery : IRequest<Result<string>>;
