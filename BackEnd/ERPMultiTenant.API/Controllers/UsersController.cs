using ERPMultiTenant.API.Authorization;
using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application.Features.Users.Delete;
using ERPMultiTenant.Application.Features.Users.Invite;
using ERPMultiTenant.Application.Features.Users.List;
using ERPMultiTenant.Application.Features.Users.Profile;
using ERPMultiTenant.Application.Features.Users.UpdateRole;
using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPMultiTenant.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    IMediator mediator,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService) : ControllerBase
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

        if (currentTenantService.CurrentTenantId is null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<UserProfileResponse>.Fail("Invalid tenant claims."));
        }

        var result = await mediator.Send(
            new UserProfileQuery(
                currentUserService.UserId.Value,
                currentUserService.Email,
                currentUserService.Role.ToString(),
                currentTenantService.CurrentTenantId.Value),
            cancellationToken);

        if (!result.Success)
        {
            return NotFound(ApiResponse<UserProfileResponse>.Fail(result.Error ?? "User not found."));
        }

        return Ok(ApiResponse<UserProfileResponse>.Ok("Profile retrieved successfully.", result.Value));
    }

    [HttpPost("invite")]
    [HasPermission(Permission.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<InviteUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InviteUserResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InviteUserResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InviteUserResponse>>> InviteUser(
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Email already exists")
            {
                return Conflict(ApiResponse<InviteUserResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<InviteUserResponse>.Fail(result.Error ?? "Invite failed."));
        }

        return Ok(ApiResponse<InviteUserResponse>.Ok("User invited successfully.", result.Value));
    }

    [HttpGet]
    [HasPermission(Permission.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserListItemResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserListItemResponse>>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUsersQuery(), cancellationToken);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<IReadOnlyList<UserListItemResponse>>.Fail(result.Error ?? "Failed to retrieve users."));
        }

        return Ok(ApiResponse<IReadOnlyList<UserListItemResponse>>.Ok("Users retrieved successfully.", result.Value));
    }

    [HttpPut("{userId:guid}/role")]
    [HasPermission(Permission.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<UpdateUserRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateUserRoleResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UpdateUserRoleResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UpdateUserRoleResponse>>> UpdateRole(
        Guid userId,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (userId != request.UserId)
        {
            return BadRequest(ApiResponse<UpdateUserRoleResponse>.Fail("User id mismatch."));
        }

        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "User not found.")
            {
                return NotFound(ApiResponse<UpdateUserRoleResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<UpdateUserRoleResponse>.Fail(result.Error ?? "Failed to update role."));
        }

        return Ok(ApiResponse<UpdateUserRoleResponse>.Ok("User role updated successfully.", result.Value));
    }

    [HttpDelete("{userId:guid}")]
    [HasPermission(Permission.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<DeleteUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeleteUserResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DeleteUserResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteUserRequest(userId), cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "User not found.")
            {
                return NotFound(ApiResponse<DeleteUserResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<DeleteUserResponse>.Fail(result.Error ?? "Failed to delete user."));
        }

        return Ok(ApiResponse<DeleteUserResponse>.Ok("User deleted successfully.", result.Value));
    }
}
