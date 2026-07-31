using QPS.Domain.Common;

namespace QPS.Domain.Entities.System;

public class SystemChinaRegion : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public byte Level { get; private set; }
    public string? ParentCode { get; private set; }
    public string? ProvinceCode { get; private set; }
    public string? CityCode { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private SystemChinaRegion() { }
}


