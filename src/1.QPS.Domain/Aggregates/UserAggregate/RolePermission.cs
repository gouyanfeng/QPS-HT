using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.UserAggregate;

public class RolePermission : Entity
{
    public Guid RoleId { get; private set; }
    public string PermissionCode { get; private set; }

    protected RolePermission() { }

    public RolePermission(Guid roleId, string permissionCode)
    {
        RoleId = roleId;
        PermissionCode = permissionCode;
    }
}