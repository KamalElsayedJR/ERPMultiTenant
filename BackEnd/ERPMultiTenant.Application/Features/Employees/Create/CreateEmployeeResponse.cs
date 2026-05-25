namespace ERPMultiTenant.Application.Features.Employees.Create;

public sealed record CreateEmployeeResponse(
    Guid EmployeeId,
    string EmployeeNumber,
    Guid DepartmentId,
    Guid ApplicationUserId,
    string? JobTitle,
    DateOnly HireDate,
    decimal Salary,
    DateTime CreatedAt);
