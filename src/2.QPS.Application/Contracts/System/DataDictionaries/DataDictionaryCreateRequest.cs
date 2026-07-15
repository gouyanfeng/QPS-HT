namespace QPS.Application.Contracts.System.DataDictionaries;

public class DataDictionaryCreateRequest
{
    public Guid? ParentId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}