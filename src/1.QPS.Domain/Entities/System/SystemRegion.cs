using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemRegion : BaseEntity
{
    public Guid? ParentId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public int Level { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    public virtual SystemRegion? Parent { get; private set; }
    public virtual ICollection<SystemRegion> Children { get; private set; } = new List<SystemRegion>();

    private SystemRegion() { }

    public SystemRegion(Guid id, Guid? parentId, string code, string name, int level, int sortOrder, bool isActive)
    {
        Id = id;
        ParentId = parentId;
        Code = code;
        Name = name;
        Level = level;
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    public void Update(Guid? parentId, string code, string name, int level, int sortOrder, bool isActive)
    {
        ParentId = parentId;
        Code = code;
        Name = name;
        Level = level;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}


