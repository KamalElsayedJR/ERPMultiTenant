namespace ERPMultiTenant.Application.Features.Employees.Update;

public sealed record UpdateEmployeeResponse(
    Guid EmployeeId,
    string EmployeeNumber,
    Guid DepartmentId,
    Guid ApplicationUserId,
    string? JobTitle,
    DateOnly HireDate,
    decimal Salary,
    DateTime UpdatedAt);
