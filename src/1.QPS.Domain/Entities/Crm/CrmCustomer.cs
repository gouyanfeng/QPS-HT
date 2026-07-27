using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

/// <summary>
/// CRM客户，来源于清洗后的线索并用于后续客户管理。
/// </summary>
public class CrmCustomer : BaseEntity
{
    private const string PendingContactStatus = "待联系";
    private const string FollowingUpStatus = "跟进中";

    /// <summary>
    /// 上级客户ID，用于维护客户层级关系。
    /// </summary>
    public Guid? ParentCustomerId { get; private set; }

    /// <summary>
    /// 客户名称，对应清洗线索名称，导入CRM使用。
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    /// 客户类型，对应线索类型，例如基地、合作社、企业、流通商、疑似无关、待判断。
    /// </summary>
    public string CustomerType { get; private set; } = string.Empty;

    /// <summary>
    /// 主营品类，例如黄芪、党参、天麻。
    /// </summary>
    public string MainProduct { get; private set; } = string.Empty;

    /// <summary>
    /// 客户等级，例如A、B、C、无效。
    /// </summary>
    public string Grade { get; private set; } = string.Empty;

    /// <summary>
    /// 线索评分，用于排序和筛选。
    /// </summary>
    public int Score { get; private set; }

    /// <summary>
    /// 省份。
    /// </summary>
    public string Province { get; private set; } = string.Empty;

    /// <summary>
    /// 城市。
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// 区县。
    /// </summary>
    public string Area { get; private set; } = string.Empty;

    /// <summary>
    /// 详细地址，限制200字符。
    /// </summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>
    /// 纬度。
    /// </summary>
    public decimal? Lat { get; private set; }

    /// <summary>
    /// 经度。
    /// </summary>
    public decimal? Lng { get; private set; }

    /// <summary>
    /// 数据来源平台，默认百度地图。
    /// </summary>
    public string SourcePlatform { get; private set; } = string.Empty;

    /// <summary>
    /// 来源表记录ID，对应BaiduPoiHerbBase.Id。
    /// </summary>
    public long? SourceLeadId { get; private set; }

    /// <summary>
    /// 客户处理状态，例如待联系、跟进中、已成交、已流失。
    /// </summary>
    public string Status { get; private set; } = string.Empty;

    /// <summary>
    /// 负责人用户ID。
    /// </summary>
    public Guid? OwnerUserId { get; private set; }

    /// <summary>
    /// 备注，例如疑似药房、电话需二次确认、合作社但无品类。
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    public string PrimaryContactName { get; private set; } = string.Empty;

    public string PrimaryContactPhone { get; private set; } = string.Empty;

    public DateTime? LastFollowAt { get; private set; }

    public string LastFollowResult { get; private set; } = string.Empty;

    public DateTime? NextFollowAt { get; private set; }

    /// <summary>
    /// 上级客户。
    /// </summary>
    public virtual CrmCustomer? ParentCustomer { get; private set; }

    /// <summary>
    /// 下级客户集合。
    /// </summary>
    public virtual ICollection<CrmCustomer> Children { get; private set; } = new List<CrmCustomer>();

    /// <summary>
    /// 客户联系人集合。
    /// </summary>
    public virtual ICollection<CrmContact> Contacts { get; private set; } = new List<CrmContact>();

    /// <summary>
    /// 客户跟进记录集合。
    /// </summary>
    public virtual ICollection<CrmFollowRecord> FollowRecords { get; private set; } = new List<CrmFollowRecord>();

    private CrmCustomer() { }

    private CrmCustomer(
        string customerName,
        string customerType,
        string mainProduct,
        string grade,
        int score,
        string province,
        string city,
        string area,
        string address,
        decimal? lat,
        decimal? lng,
        string sourcePlatform,
        long? sourceLeadId,
        Guid? ownerUserId,
        string remark,
        Guid? parentCustomerId)
    {
        CustomerName = customerName;
        CustomerType = customerType;
        MainProduct = mainProduct;
        Grade = grade;
        Score = score;
        Province = province;
        City = city;
        Area = area;
        Address = address;
        Lat = lat;
        Lng = lng;
        SourcePlatform = sourcePlatform;
        SourceLeadId = sourceLeadId;
        OwnerUserId = ownerUserId;
        Remark = remark;
        ParentCustomerId = parentCustomerId;
        Status = PendingContactStatus;
    }

    public static CrmCustomer Create(
        string customerName,
        string customerType,
        string mainProduct,
        string grade,
        int score,
        string province,
        string city,
        string area,
        string address,
        decimal? lat,
        decimal? lng,
        string sourcePlatform,
        long? sourceLeadId,
        Guid? ownerUserId,
        string remark,
        Guid? parentCustomerId = null)
    {
        return new CrmCustomer(
            customerName,
            customerType,
            mainProduct,
            grade,
            score,
            province,
            city,
            area,
            address,
            lat,
            lng,
            sourcePlatform,
            sourceLeadId,
            ownerUserId,
            remark,
            parentCustomerId);
    }

    public void UpdateBasicInfo(
        string customerName,
        string customerType,
        string mainProduct,
        string grade,
        int score,
        string province,
        string city,
        string area,
        string address,
        decimal? lat,
        decimal? lng,
        string remark)
    {
        CustomerName = customerName;
        CustomerType = customerType;
        MainProduct = mainProduct;
        Grade = grade;
        Score = score;
        Province = province;
        City = city;
        Area = area;
        Address = address;
        Lat = lat;
        Lng = lng;
        Remark = remark;
    }

    public void SetParent(Guid? parentCustomerId)
    {
        ParentCustomerId = parentCustomerId;
    }

    public void AssignOwner(Guid? ownerUserId)
    {
        OwnerUserId = ownerUserId;
    }

    public void UpdatePrimaryContact(string contactName, string phone)
    {
        PrimaryContactName = contactName;
        PrimaryContactPhone = phone;
    }

    public void ClearPrimaryContact()
    {
        PrimaryContactName = string.Empty;
        PrimaryContactPhone = string.Empty;
    }

    public void UpdateFollowSummary(DateTime followAt, string followResult, DateTime? nextFollowAt)
    {
        LastFollowAt = followAt;
        LastFollowResult = followResult;
        NextFollowAt = nextFollowAt;

        if (Status == PendingContactStatus)
        {
            Status = FollowingUpStatus;
        }
    }

    public void UpdateStatus(string status, string remark)
    {
        Status = status;
        Remark = remark;
    }
}
