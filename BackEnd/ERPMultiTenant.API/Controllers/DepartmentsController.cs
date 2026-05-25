using ERPMultiTenant.API.Authorization;
using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application.Features.Departments.Create;
using ERPMultiTenant.Application.Features.Departments.Delete;
using ERPMultiTenant.Application.Features.Departments.List;
using ERPMultiTenant.Application.Features.Departments.Update;
using ERPMultiTenant.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPMultiTenant.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public sealed class DepartmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permission.DepartmentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentListItemResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<DepartmentListItemResponse>>>> GetDepartments(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDepartmentsQuery(pageNumber, pageSize), cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return BadRequest(ApiResponse<PaginatedResponse<DepartmentListItemResponse>>.Fail(result.Error ?? "Failed to retrieve departments."));
        }

        var paged = new PaginatedResponse<DepartmentListItemResponse>(
            result.Value.Items,
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalCount);

        return Ok(ApiResponse<PaginatedResponse<DepartmentListItemResponse>>.Ok("Departments retrieved successfully.", paged));
    }

    [HttpPost]
    [HasPermission(Permission.DepartmentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<CreateDepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateDepartmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateDepartmentResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CreateDepartmentResponse>>> CreateDepartment(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Department name already exists.")
            {
                return Conflict(ApiResponse<CreateDepartmentResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<CreateDepartmentResponse>.Fail(result.Error ?? "Failed to create department."));
        }

        return Ok(ApiResponse<CreateDepartmentResponse>.Ok("Department created successfully.", result.Value));
    }

    [HttpPut("{departmentId:guid}")]
    [HasPermission(Permission.DepartmentsUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UpdateDepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateDepartmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UpdateDepartmentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UpdateDepartmentResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UpdateDepartmentResponse>>> UpdateDepartment(
        Guid departmentId,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        if (departmentId != request.DepartmentId)
        {
            return BadRequest(ApiResponse<UpdateDepartmentResponse>.Fail("Department id mismatch."));
        }

        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Department not found.")
            {
                return NotFound(ApiResponse<UpdateDepartmentResponse>.Fail(result.Error));
            }

            if (result.Error == "Department name already exists.")
            {
                return Conflict(ApiResponse<UpdateDepartmentResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<UpdateDepartmentResponse>.Fail(result.Error ?? "Failed to update department."));
        }

        return Ok(ApiResponse<UpdateDepartmentResponse>.Ok("Department updated successfully.", result.Value));
    }

    [HttpDelete("{departmentId:guid}")]
    [HasPermission(Permission.DepartmentsDelete)]
    [ProducesResponseType(typeof(ApiResponse<DeleteDepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeleteDepartmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DeleteDepartmentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DeleteDepartmentResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<DeleteDepartmentResponse>>> DeleteDepartment(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteDepartmentRequest(departmentId), cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Department not found.")
            {
                return NotFound(ApiResponse<DeleteDepartmentResponse>.Fail(result.Error));
            }

            if (result.Error == "Department has employees.")
            {
                return Conflict(ApiResponse<DeleteDepartmentResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<DeleteDepartmentResponse>.Fail(result.Error ?? "Failed to delete department."));
        }

        return Ok(ApiResponse<DeleteDepartmentResponse>.Ok("Department deleted successfully.", result.Value));
    }
}
