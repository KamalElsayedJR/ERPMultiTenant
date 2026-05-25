using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Update;

public sealed class UpdateEmployeeRequestHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IUserRepository userRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<UpdateEmployeeRequest, Result<UpdateEmployeeResponse>>
{
    public async Task<Result<UpdateEmployeeResponse>> Handle(UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<UpdateEmployeeResponse>.Fail("Tenant context is missing.");
        }

        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, tenantId.Value, cancellationToken);
        if (employee is null)
        {
            return Result<UpdateEmployeeResponse>.Fail("Employee not found.");
        }

        var normalizedEmployeeNumber = request.EmployeeNumber.Trim();
        if (await employeeRepository.EmployeeNumberExistsAsync(tenantId.Value, normalizedEmployeeNumber, employee.Id, cancellationToken))
        {
            return Result<UpdateEmployeeResponse>.Fail("Employee number already exists.");
        }

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, tenantId.Value, cancellationToken);
        if (department is null)
        {
            return Result<UpdateEmployeeResponse>.Fail("Department not found.");
        }

        var applicationUser = await userRepository.GetByIdAsync(request.ApplicationUserId, tenantId.Value, cancellationToken);
        if (applicationUser is null)
        {
            return Result<UpdateEmployeeResponse>.Fail("Application user not found.");
        }

        if (await employeeRepository.ApplicationUserAssignedAsync(tenantId.Value, request.ApplicationUserId, employee.Id, cancellationToken))
        {
            return Result<UpdateEmployeeResponse>.Fail("Application user is already linked to an employee.");
        }

        employee.EmployeeNumber = normalizedEmployeeNumber;
        employee.JobTitle = request.JobTitle?.Trim();
        employee.HireDate = request.HireDate;
        employee.Salary = request.Salary;
        employee.DepartmentId = department.Id;
        employee.ApplicationUserId = applicationUser.Id;
        employee.UpdatedAt = DateTime.UtcNow;

        await employeeRepository.UpdateAsync(employee, cancellationToken);

        var updatedAt = employee.UpdatedAt ?? DateTime.UtcNow;
        var response = new UpdateEmployeeResponse(
            employee.Id,
            employee.EmployeeNumber,
            employee.DepartmentId,
            employee.ApplicationUserId,
            employee.JobTitle,
            employee.HireDate,
            employee.Salary,
            updatedAt);

        return Result<UpdateEmployeeResponse>.Ok(response);
    }
}
