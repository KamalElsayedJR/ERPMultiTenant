using ERPMultiTenant.API.Authorization;
using ERPMultiTenant.API.Responses;
using ERPMultiTenant.Application.Features.Employees.Create;
using ERPMultiTenant.Application.Features.Employees.Delete;
using ERPMultiTenant.Application.Features.Employees.Details;
using ERPMultiTenant.Application.Features.Employees.List;
using ERPMultiTenant.Application.Features.Employees.Update;
using ERPMultiTenant.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPMultiTenant.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public sealed class EmployeesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permission.EmployeesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<EmployeeListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<EmployeeListItemResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<EmployeeListItemResponse>>>> GetEmployees(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? departmentId,
        [FromQuery] decimal? minSalary,
        [FromQuery] decimal? maxSalary,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetEmployeesQuery(pageNumber, pageSize, searchTerm, departmentId, minSalary, maxSalary, sortBy, sortDirection),
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return BadRequest(ApiResponse<PaginatedResponse<EmployeeListItemResponse>>.Fail(result.Error ?? "Failed to retrieve employees."));
        }

        var paged = new PaginatedResponse<EmployeeListItemResponse>(
            result.Value.Items,
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalCount);

        return Ok(ApiResponse<PaginatedResponse<EmployeeListItemResponse>>.Ok("Employees retrieved successfully.", paged));
    }

    [HttpGet("{employeeId:guid}")]
    [HasPermission(Permission.EmployeesView)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailsResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailsResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EmployeeDetailsResponse>>> GetEmployeeDetails(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeDetailsQuery(employeeId), cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Employee not found.")
            {
                return NotFound(ApiResponse<EmployeeDetailsResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<EmployeeDetailsResponse>.Fail(result.Error ?? "Failed to retrieve employee."));
        }

        return Ok(ApiResponse<EmployeeDetailsResponse>.Ok("Employee retrieved successfully.", result.Value));
    }

    [HttpPost]
    [HasPermission(Permission.EmployeesCreate)]
    [ProducesResponseType(typeof(ApiResponse<CreateEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateEmployeeResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateEmployeeResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CreateEmployeeResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CreateEmployeeResponse>>> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Employee number already exists." || result.Error == "Application user is already linked to an employee.")
            {
                return Conflict(ApiResponse<CreateEmployeeResponse>.Fail(result.Error));
            }

            if (result.Error == "Department not found." || result.Error == "Application user not found.")
            {
                return NotFound(ApiResponse<CreateEmployeeResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<CreateEmployeeResponse>.Fail(result.Error ?? "Failed to create employee."));
        }

        return Ok(ApiResponse<CreateEmployeeResponse>.Ok("Employee created successfully.", result.Value));
    }

    [HttpPut("{employeeId:guid}")]
    [HasPermission(Permission.EmployeesUpdate)]
    [ProducesResponseType(typeof(ApiResponse<UpdateEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateEmployeeResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UpdateEmployeeResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UpdateEmployeeResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UpdateEmployeeResponse>>> UpdateEmployee(
        Guid employeeId,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (employeeId != request.EmployeeId)
        {
            return BadRequest(ApiResponse<UpdateEmployeeResponse>.Fail("Employee id mismatch."));
        }

        var result = await mediator.Send(request, cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Employee not found." || result.Error == "Department not found." || result.Error == "Application user not found.")
            {
                return NotFound(ApiResponse<UpdateEmployeeResponse>.Fail(result.Error));
            }

            if (result.Error == "Employee number already exists." || result.Error == "Application user is already linked to an employee.")
            {
                return Conflict(ApiResponse<UpdateEmployeeResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<UpdateEmployeeResponse>.Fail(result.Error ?? "Failed to update employee."));
        }

        return Ok(ApiResponse<UpdateEmployeeResponse>.Ok("Employee updated successfully.", result.Value));
    }

    [HttpDelete("{employeeId:guid}")]
    [HasPermission(Permission.EmployeesDelete)]
    [ProducesResponseType(typeof(ApiResponse<DeleteEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeleteEmployeeResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DeleteEmployeeResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DeleteEmployeeResponse>>> DeleteEmployee(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteEmployeeRequest(employeeId), cancellationToken);

        if (!result.Success)
        {
            if (result.Error == "Employee not found.")
            {
                return NotFound(ApiResponse<DeleteEmployeeResponse>.Fail(result.Error));
            }

            return BadRequest(ApiResponse<DeleteEmployeeResponse>.Fail(result.Error ?? "Failed to delete employee."));
        }

        return Ok(ApiResponse<DeleteEmployeeResponse>.Ok("Employee deleted successfully.", result.Value));
    }
}
