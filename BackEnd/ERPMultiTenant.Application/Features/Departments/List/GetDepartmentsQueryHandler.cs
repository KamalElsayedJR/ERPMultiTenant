using ERPMultiTenant.Application.Interfaces.Authentication;
using ERPMultiTenant.Application.Interfaces.Persistence;
using ERPMultiTenant.Application.Models;
using MediatR;

namespace ERPMultiTenant.Application.Features.Departments.List;

public sealed class GetDepartmentsQueryHandler(
    IDepartmentRepository departmentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetDepartmentsQuery, Result<DepartmentListResponse>>
{
    public async Task<Result<DepartmentListResponse>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.CurrentTenantId;
        if (tenantId is null)
        {
            return Result<DepartmentListResponse>.Fail("Tenant context is missing.");
        }

        var pageNumber = request.PageNumber.GetValueOrDefault(1);
        var pageSize = request.PageSize.GetValueOrDefault(25);

        var totalCount = await departmentRepository.CountByTenantIdAsync(tenantId.Value, cancellationToken);
        var departments = await departmentRepository.GetPagedByTenantIdAsync(tenantId.Value, pageNumber, pageSize, cancellationToken);

        var items = departments
            .Select(department => new DepartmentListItemResponse(
                department.Id,
                department.Name,
                department.Description,
                department.CreatedAt))
            .ToList()
            .AsReadOnly();

        var response = new DepartmentListResponse(items, pageNumber, pageSize, totalCount);
        return Result<DepartmentListResponse>.Ok(response);
    }
}
