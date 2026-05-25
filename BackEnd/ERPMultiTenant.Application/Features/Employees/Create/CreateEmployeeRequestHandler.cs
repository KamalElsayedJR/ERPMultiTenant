using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Entities;
using MediatR;

namespace ERPMultiTenant.Application.Features.Employees.Create;

public sealed class CreateEmployeeRequestHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IUserRepository userRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CreateEmployeeRequest, Result<CreateEmployeeResponse>>
{
    public async Task<Result<CreateEmployeeResponse>> Handle(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<CreateEmployeeResponse>.Fail("Tenant context is missing.");
        }

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, tenantId.Value, cancellationToken);
        if (department is null)
        {
            return Result<CreateEmployeeResponse>.Fail("Department not found.");
        }

        var departmentCode = BuildDepartmentCode(department.Name);
        var nextSequence = await employeeRepository.GetNextEmployeeNumberSequenceAsync(tenantId.Value, department.Id, cancellationToken);
        var employeeNumber = $"{departmentCode}-{nextSequence:D4}";

        if (await employeeRepository.EmployeeNumberExistsAsync(tenantId.Value, employeeNumber, null, cancellationToken))
        {
            return Result<CreateEmployeeResponse>.Fail("Employee number already exists.");
        }

        var applicationUser = await userRepository.GetByIdAsync(request.ApplicationUserId, tenantId.Value, cancellationToken);
        if (applicationUser is null)
        {
            return Result<CreateEmployeeResponse>.Fail("Application user not found.");
        }

        if (await employeeRepository.ApplicationUserAssignedAsync(tenantId.Value, request.ApplicationUserId, null, cancellationToken))
        {
            return Result<CreateEmployeeResponse>.Fail("Application user is already linked to an employee.");
        }

        var employee = new Employee
        {
            EmployeeNumber = employeeNumber,
            JobTitle = request.JobTitle?.Trim(),
            HireDate = request.HireDate,
            Salary = request.Salary,
            TenantId = tenantId.Value,
            DepartmentId = department.Id,
            ApplicationUserId = applicationUser.Id
        };

        await employeeRepository.AddAsync(employee, cancellationToken);

        var response = new CreateEmployeeResponse(
            employee.Id,
            employee.EmployeeNumber,
            employee.DepartmentId,
            employee.ApplicationUserId,
            employee.JobTitle,
            employee.HireDate,
            employee.Salary,
            employee.CreatedAt);

        return Result<CreateEmployeeResponse>.Ok(response);
    }

    private static string BuildDepartmentCode(string departmentName)
    {
        var trimmed = departmentName.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return "DEP";
        }

        var letters = trimmed
            .Where(char.IsLetter)
            .Select(char.ToUpperInvariant)
            .ToArray();

        if (letters.Length == 0)
        {
            return "DEP";
        }

        return new string(letters.Length >= 2 ? letters[..2] : letters);
    }
}
