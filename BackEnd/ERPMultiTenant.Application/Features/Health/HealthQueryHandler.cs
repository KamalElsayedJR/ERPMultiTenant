using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Health;

public sealed class HealthQueryHandler : IRequestHandler<HealthQuery, Result<string>>
{
    public Task<Result<string>> Handle(HealthQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<string>.Ok("ERP API Running"));
    }
}
