namespace ERPMultiTenant.Application.Features.Departments.List;

public sealed record DepartmentListResponse(
    IReadOnlyList<DepartmentListItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);
