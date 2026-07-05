using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class SystemPermission : BaseEntity
{
    public string PermissionCode { get; private set; }
    public string Name { get; private set; }
    public Guid? ParentId { get; private set; }

    protected SystemPermission() { }

    public SystemPermission(string permissionCode, string name, Guid? parentId = null)
    {
        PermissionCode = permissionCode;
        Name = name;
        ParentId = parentId;
    }

    public void Update(string permissionCode, string name, Guid? parentId = null)
    {
        PermissionCode = permissionCode;
        Name = name;
        ParentId = parentId;
    }
}