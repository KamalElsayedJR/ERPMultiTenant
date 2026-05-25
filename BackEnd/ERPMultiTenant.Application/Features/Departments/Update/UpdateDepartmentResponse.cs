namespace ERPMultiTenant.Application.Features.Departments.Update;

public sealed record UpdateDepartmentResponse(Guid DepartmentId, string Name, string? Description, DateTime UpdatedAt);
