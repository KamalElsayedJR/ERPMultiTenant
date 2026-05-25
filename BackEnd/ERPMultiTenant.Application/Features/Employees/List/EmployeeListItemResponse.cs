namespace ERPMultiTenant.Application.Features.Employees.List;

public sealed record EmployeeListItemResponse(
    Guid EmployeeId,
    string EmployeeNumber,
    string FullName,
    string Email,
    Guid DepartmentId,
    string DepartmentName,
    string? JobTitle,
    DateOnly HireDate,
    decimal Salary);
