namespace QPS.Application.Contracts.System.DataDictionaries;

public class DataDictionaryUpdateRequest
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}