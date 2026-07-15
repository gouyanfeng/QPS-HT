using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemPermission : BaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public Guid? ParentId { get; private set; }

    protected SystemPermission() { }

    public SystemPermission(string name, string code, Guid? parentId = null)
    {
        Name = name;
        Code = code;
        ParentId = parentId;
    }

    public void Update(string name, string code, Guid? parentId = null)
    {
        Name = name;
        Code = code;
        ParentId = parentId;
    }
}