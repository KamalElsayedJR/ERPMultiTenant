using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application.Features.Health;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPMultiTenant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<string>>> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new HealthQuery(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(result.Error ?? "Service unavailable."));
        }

        return Ok(ApiResponse<string>.Ok(result.Value ?? "ERP API Running"));
    }
}
