using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application.Features.Authentication.Login;
using ERPMultiTenant.Application.Features.Authentication.Logout;
using ERPMultiTenant.Application.Features.Authentication.RefreshToken;
using ERPMultiTenant.Application.Features.Authentication.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPMultiTenant.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Email is already registered.")
            {
                return Conflict(ApiResponse<RegisterResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<RegisterResponse>.Fail(result.Error ?? "Registration failed."));
        }

        return Ok(ApiResponse<RegisterResponse>.Ok("User registered successfully.", result.Value));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<LoginResponse>.Fail(result.Error ?? "Invalid credentials."));
        }

        return Ok(ApiResponse<LoginResponse>.Ok("Login successful.", result.Value));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<RefreshTokenResponse>.Fail(result.Error ?? "Invalid token."));
        }

        return Ok(ApiResponse<RefreshTokenResponse>.Ok("Token refreshed successfully.", result.Value));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> Logout(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LogoutRequest(), cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Unauthorized")
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<string>.Fail(result.Error));
            }

            return NotFound(ApiResponse<string>.Fail(result.Error ?? "Logout failed."));
        }

        return Ok(ApiResponse<string>.Ok(result.Value ?? "Logged out successfully."));
    }
}
