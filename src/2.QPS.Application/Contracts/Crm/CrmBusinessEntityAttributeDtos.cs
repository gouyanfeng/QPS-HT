namespace QPS.Application.Contracts.Crm;

public class CrmBusinessEntityAttributeDto
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string AttributeCode { get; set; } = string.Empty;

    public string AttributeValue { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string Remark { get; set; } = string.Empty;
}

public class AttributeOptionDto
{
    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class CrmBusinessEntityAttributeSaveRequest
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string AttributeCode { get; set; } = string.Empty;

    public List<string> Values { get; set; } = new();
}
