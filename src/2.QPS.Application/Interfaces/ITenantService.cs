namespace QPS.Application.Interfaces;

public interface ITenantService
{
    Guid GetCurrentTenantId();
    void SetTenantId(Guid tenantId);
}