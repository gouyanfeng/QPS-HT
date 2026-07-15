using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemRolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    protected SystemRolePermission() { }

    public SystemRolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}