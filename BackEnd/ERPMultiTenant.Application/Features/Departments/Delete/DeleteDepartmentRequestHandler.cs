using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.Delete;

public sealed class DeleteDepartmentRequestHandler(
    IDepartmentRepository departmentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<DeleteDepartmentRequest, Result<DeleteDepartmentResponse>>
{
    public async Task<Result<DeleteDepartmentResponse>> Handle(DeleteDepartmentRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<DeleteDepartmentResponse>.Fail("Tenant context is missing.");
        }

        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, tenantId.Value, cancellationToken);
        if (department is null)
        {
            return Result<DeleteDepartmentResponse>.Fail("Department not found.");
        }

        if (await departmentRepository.HasEmployeesAsync(department.Id, tenantId.Value, cancellationToken))
        {
            return Result<DeleteDepartmentResponse>.Fail("Department has employees.");
        }

        await departmentRepository.DeleteAsync(department, cancellationToken);

        return Result<DeleteDepartmentResponse>.Ok(new DeleteDepartmentResponse(department.Id));
    }
}
