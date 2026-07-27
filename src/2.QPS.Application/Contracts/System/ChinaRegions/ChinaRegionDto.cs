namespace QPS.Application.Contracts.System.ChinaRegions;

public class ChinaRegionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? ParentCode { get; set; }
    public string? ProvinceCode { get; set; }
    public string? CityCode { get; set; }
    public int SortOrder { get; set; }
}
