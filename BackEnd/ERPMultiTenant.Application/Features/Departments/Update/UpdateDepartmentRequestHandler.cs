using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.Update;

public sealed class UpdateDepartmentRequestHandler(
    IDepartmentRepository departmentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<UpdateDepartmentRequest, Result<UpdateDepartmentResponse>>
{
    public async Task<Result<UpdateDepartmentResponse>> Handle(UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<UpdateDepartmentResponse>.Fail("Tenant context is missing.");
        }

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, tenantId.Value, cancellationToken);
        if (department is null)
        {
            return Result<UpdateDepartmentResponse>.Fail("Department not found.");
        }

        var normalizedName = request.Name.Trim();
        if (await departmentRepository.NameExistsAsync(tenantId.Value, normalizedName, department.Id, cancellationToken))
        {
            return Result<UpdateDepartmentResponse>.Fail("Department name already exists.");
        }

        department.Name = normalizedName;
        department.Description = request.Description?.Trim();
        department.UpdatedAt = DateTime.UtcNow;

        await departmentRepository.UpdateAsync(department, cancellationToken);

        var updatedAt = department.UpdatedAt ?? DateTime.UtcNow;
        var response = new UpdateDepartmentResponse(department.Id, department.Name, department.Description, updatedAt);
        return Result<UpdateDepartmentResponse>.Ok(response);
    }
}
