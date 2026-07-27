namespace QPS.Application.Contracts.System.Regions;

public class RegionDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
