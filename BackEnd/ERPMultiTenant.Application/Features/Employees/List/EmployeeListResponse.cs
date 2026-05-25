namespace ERPMultiTenant.Application.Features.Employees.List;

public sealed record EmployeeListResponse(
    IReadOnlyList<EmployeeListItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    string? SearchTerm,
    Guid? DepartmentId,
    decimal? MinSalary,
    decimal? MaxSalary,
    string? SortBy,
    string? SortDirection);
