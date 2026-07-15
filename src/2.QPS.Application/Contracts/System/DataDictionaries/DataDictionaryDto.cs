namespace QPS.Application.Contracts.System.DataDictionaries;

public class DataDictionaryDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid MerchantId { get; set; }
    public List<DataDictionaryDto> Children { get; set; } = new List<DataDictionaryDto>();
}