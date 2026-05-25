namespace ERPMultiTenant.Application.Features.Departments.List;

public sealed record DepartmentListItemResponse(Guid DepartmentId, string Name, string? Description, DateTime CreatedAt);
