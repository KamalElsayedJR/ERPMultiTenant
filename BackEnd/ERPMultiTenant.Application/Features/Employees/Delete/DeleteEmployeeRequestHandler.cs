using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Delete;

public sealed class DeleteEmployeeRequestHandler(
    IEmployeeRepository employeeRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<DeleteEmployeeRequest, Result<DeleteEmployeeResponse>>
{
    public async Task<Result<DeleteEmployeeResponse>> Handle(DeleteEmployeeRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<DeleteEmployeeResponse>.Fail("Tenant context is missing.");
        }

        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, tenantId.Value, cancellationToken);
        if (employee is null)
        {
            return Result<DeleteEmployeeResponse>.Fail("Employee not found.");
        }

        await employeeRepository.DeleteAsync(employee, cancellationToken);

        return Result<DeleteEmployeeResponse>.Ok(new DeleteEmployeeResponse(employee.Id));
    }
}
