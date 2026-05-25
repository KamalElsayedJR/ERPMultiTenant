using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using ERPMultiTenant.Domain.Entities;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.Create;

public sealed class CreateDepartmentRequestHandler(
    IDepartmentRepository departmentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CreateDepartmentRequest, Result<CreateDepartmentResponse>>
{
    public async Task<Result<CreateDepartmentResponse>> Handle(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<CreateDepartmentResponse>.Fail("Tenant context is missing.");
        }

        var normalizedName = request.Name.Trim();
        if (await departmentRepository.NameExistsAsync(tenantId.Value, normalizedName, null, cancellationToken))
        {
            return Result<CreateDepartmentResponse>.Fail("Department name already exists.");
        }

        var department = new Department
        {
            Name = normalizedName,
            Description = request.Description?.Trim(),
            TenantId = tenantId.Value
        };

        await departmentRepository.AddAsync(department, cancellationToken);

        var response = new CreateDepartmentResponse(department.Id, department.Name, department.Description, department.CreatedAt);
        return Result<CreateDepartmentResponse>.Ok(response);
    }
}
