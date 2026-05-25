namespace ERPMultiTenant.Application.Features.Departments.Create;

public sealed record CreateDepartmentResponse(Guid DepartmentId, string Name, string? Description, DateTime CreatedAt);
