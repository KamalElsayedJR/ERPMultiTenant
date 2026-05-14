namespace ERPMultiTenant.Application.Interfaces.Authentication;

public interface ICurrentTenantService
{
    Guid? CurrentTenantId { get; }
}
