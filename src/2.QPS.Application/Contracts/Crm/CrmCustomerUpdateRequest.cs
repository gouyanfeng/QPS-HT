namespace QPS.Application.Contracts.Crm;

/// <summary>
/// 更新CRM客户请求。
/// </summary>
public class CrmCustomerUpdateRequest
{
    /// <summary>
    /// 上级客户ID，用于维护客户层级关系。
    /// </summary>
    public Guid? ParentCustomerId { get; set; }

    /// <summary>
    /// 客户名称，对应清洗线索名称，导入CRM使用。
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// 客户类型，对应线索类型，例如基地、合作社、企业、流通商、疑似无关、待判断。
    /// </summary>
    public string CustomerType { get; set; } = string.Empty;

    /// <summary>
    /// 主营品类，例如黄芪、党参、天麻。
    /// </summary>
    public string MainProduct { get; set; } = string.Empty;

    /// <summary>
    /// 客户等级，例如A、B、C、无效。
    /// </summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>
    /// 线索评分，用于排序和筛选。
    /// </summary>
    public int Score { get; set; }

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
    public long? SourceLeadId { get; set; }

    /// <summary>
    /// 负责人用户ID。
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    /// <summary>
    /// 客户处理状态，例如待联系、跟进中、已成交、已流失。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public string? PrimaryContactName { get; set; }

    public string? PrimaryContactPhone { get; set; }

    /// <summary>
    /// 备注，例如疑似药房、电话需二次确认、合作社但无品类。
    /// </summary>
    public string Remark { get; set; } = string.Empty;
}
