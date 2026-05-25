using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Details;

public sealed class GetEmployeeDetailsQueryHandler(
    IEmployeeRepository employeeRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetEmployeeDetailsQuery, Result<EmployeeDetailsResponse>>
{
    public async Task<Result<EmployeeDetailsResponse>> Handle(GetEmployeeDetailsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<EmployeeDetailsResponse>.Fail("Tenant context is missing.");
        }

        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, tenantId.Value, cancellationToken);
        if (employee is null)
        {
            return Result<EmployeeDetailsResponse>.Fail("Employee not found.");
        }

        var response = new EmployeeDetailsResponse(
            employee.Id,
            employee.EmployeeNumber,
            employee.ApplicationUser?.FullName ?? string.Empty,
            employee.ApplicationUser?.Email ?? string.Empty,
            employee.DepartmentId,
            employee.Department?.Name ?? string.Empty,
            employee.ApplicationUserId,
            employee.JobTitle,
            employee.HireDate,
            employee.Salary,
            employee.CreatedAt,
            employee.UpdatedAt);

        return Result<EmployeeDetailsResponse>.Ok(response);
    }
}
