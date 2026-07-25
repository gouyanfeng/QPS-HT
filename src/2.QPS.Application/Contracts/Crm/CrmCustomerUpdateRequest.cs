namespace QPS.Application.Contracts.Crm;

/// <summary>
/// 更新客户请求
/// </summary>
public class CrmCustomerUpdateRequest
{
    /// <summary>
    /// 上级客户ID
    /// </summary>
    public Guid? ParentCustomerId { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// 客户类型
    /// </summary>
    public string CustomerType { get; set; } = string.Empty;

    /// <summary>
    /// 主营产品
    /// </summary>
    public string MainProduct { get; set; } = string.Empty;

    /// <summary>
    /// 客户等级
    /// </summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>
    /// 客户积分
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 区县
    /// </summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// 详细地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 纬度
    /// </summary>
    public decimal? Lat { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    public decimal? Lng { get; set; }

    /// <summary>
    /// 来源平台
    /// </summary>
    public string SourcePlatform { get; set; } = string.Empty;

    /// <summary>
    /// 来源线索ID
    /// </summary>
    public long? SourceLeadId { get; set; }

    /// <summary>
    /// 负责人用户ID
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    /// <summary>
    /// 客户状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = string.Empty;
}