namespace QPS.Application.Contracts.Crm;

/// <summary>
/// 创建CRM药材基地请求。
/// </summary>
public class CrmHerbBaseCreateRequest
{
    public Guid? HerbBaseSubjectId { get; set; }

    /// <summary>
    /// 基地名称，对应清洗线索名称，导入CRM使用。
    /// </summary>
    public string BaseName { get; set; } = string.Empty;

    /// <summary>
    /// 兼容旧接口字段，等同于基地名称。
    /// </summary>
    public string HerbBaseName { get; set; } = string.Empty;

    /// <summary>
    /// 主体名称，用于记录客户对应的工商或经营主体。
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;

    public List<string> MainProducts { get; set; } = new();

    /// <summary>
    /// 药材基地等级，例如A、B、C、无效。
    /// </summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>
    /// 线索评分，用于排序和筛选。
    /// </summary>
    public int Score { get; set; }

    public decimal? Scale { get; set; }

    /// <summary>
    /// 省份。
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// 城市。
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 区县。
    /// </summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// 详细地址，限制200字符。
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 纬度。
    /// </summary>
    public decimal? Lat { get; set; }

    /// <summary>
    /// 经度。
    /// </summary>
    public decimal? Lng { get; set; }

    /// <summary>
    /// 数据来源平台，默认百度地图。
    /// </summary>
    public string SourcePlatform { get; set; } = string.Empty;

    /// <summary>
    /// 来源表记录ID，对应BaiduPoiHerbBase.Id。
    /// </summary>
    public long? SourceId { get; set; }

    /// <summary>
    /// 负责人用户ID。
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    public string? PrimaryContactName { get; set; }

    public string? PrimaryContactPhone { get; set; }

    /// <summary>
    /// 备注，例如疑似药房、电话需二次确认、合作社但无品类。
    /// </summary>
    public string Remark { get; set; } = string.Empty;
}



