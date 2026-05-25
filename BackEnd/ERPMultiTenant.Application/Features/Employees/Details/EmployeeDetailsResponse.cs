namespace ERPMultiTenant.Application.Features.Employees.Details;

public sealed record EmployeeDetailsResponse(
    Guid EmployeeId,
    string EmployeeNumber,
    string FullName,
    string Email,
    Guid DepartmentId,
    string DepartmentName,
    Guid ApplicationUserId,
    string? JobTitle,
    DateOnly HireDate,
    decimal Salary,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
