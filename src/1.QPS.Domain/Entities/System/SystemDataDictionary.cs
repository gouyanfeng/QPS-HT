using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemDataDictionary : BaseEntity
{
    public Guid? ParentId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Value { get; private set; }
    public string Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    public virtual SystemDataDictionary? Parent { get; private set; }
    public virtual ICollection<SystemDataDictionary> Children { get; private set; } = new List<SystemDataDictionary>();

    private SystemDataDictionary() { }

    public SystemDataDictionary(Guid id, string code, string name, string value, string description,
        int sortOrder, bool isActive, Guid? parentId = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Value = value;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        ParentId = parentId;
    }

    public void Update(string name, string value, string description, int sortOrder, bool isActive, Guid? parentId = null)
    {
        Name = name;
        Value = value;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        ParentId = parentId;
    }

    public void ToggleStatus()
    {
        IsActive = !IsActive;
    }
}
