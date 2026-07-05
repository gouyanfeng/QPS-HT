using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class SystemUserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    protected SystemUserRole() { }

    public SystemUserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}