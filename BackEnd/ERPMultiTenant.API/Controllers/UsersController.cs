using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application.Features.Users.Profile;
using ERPMultiTenant.Application.Interfaces.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPMultiTenant.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile(CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated ||
            currentUserService.UserId is null ||
            string.IsNullOrWhiteSpace(currentUserService.Email) ||
            string.IsNullOrWhiteSpace(currentUserService.Role))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<UserProfileResponse>.Fail("Invalid token claims."));
        }

        var result = await mediator.Send(
            new UserProfileQuery(currentUserService.UserId.Value, currentUserService.Email, currentUserService.Role.ToString()),
            cancellationToken);

        if (!result.Success)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(result.Error ?? "User not found."));
        }

        return Ok(ApiResponse<UserProfileResponse>.Ok("Profile retrieved successfully.", result.Value));
    }
}
