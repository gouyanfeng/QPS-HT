using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class RolePermission : BaseEntity
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