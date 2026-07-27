namespace QPS.Application.Contracts.System.Regions;

public class RegionUpdateRequest
{
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
