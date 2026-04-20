using QPS.Application.Interfaces;

namespace QPS.Infrastructure.Identity;

public class TenantService : ITenantService
{
    private Guid _tenantId;

    public Guid GetCurrentTenantId()
    {
        return _tenantId;
    }

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}