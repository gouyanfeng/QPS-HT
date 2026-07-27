using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class BusinessEntityAttribute : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public string AttributeCode { get; private set; } = string.Empty;

    public string AttributeValue { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public string Remark { get; private set; } = string.Empty;

    private BusinessEntityAttribute()
    {
    }

    public BusinessEntityAttribute(
        string entityType,
        Guid entityId,
        string attributeCode,
        string attributeValue,
        int sortOrder = 0,
        string remark = "")
    {
        Update(entityType, entityId, attributeCode, attributeValue, sortOrder, remark);
    }

    public void Update(
        string entityType,
        Guid entityId,
        string attributeCode,
        string attributeValue,
        int sortOrder,
        string remark)
    {
        EntityType = entityType;
        EntityId = entityId;
        AttributeCode = attributeCode;
        AttributeValue = attributeValue;
        SortOrder = sortOrder;
        Remark = remark;
    }
}
