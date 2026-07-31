using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

/// <summary>
/// CRM药材基地，来源于清洗后的线索并用于后续药材基地管理。
/// </summary>
public class CrmHerbBase : BaseEntity
{
    private const string PendingContactStatus = "PENDING";
    private const string FollowingUpStatus = "FOLLOWING";

    /// <summary>
    /// 上级客户ID，用于维护客户层级关系。
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// 基地名称，对应清洗线索名称，导入CRM使用。
    /// </summary>
    public string BaseName { get; private set; } = string.Empty;

    /// <summary>
    /// 兼容旧接口字段，等同于基地名称。
    /// </summary>
    public string HerbBaseName => BaseName;

    /// <summary>
    /// 主体名称，用于记录客户对应的工商或经营主体。
    /// </summary>
    public string SubjectName { get; private set; } = string.Empty;

    /// <summary>
    /// 主营品类，例如黄芪、党参、天麻。
    /// </summary>
    public string MainProduct { get; private set; } = string.Empty;

    /// <summary>
    /// 药材基地等级，例如A、B、C、INVALID。
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
    /// 数据来源平台，默认BAIDU_MAP。
    /// </summary>
    public string SourcePlatform { get; private set; } = string.Empty;

    /// <summary>
    /// 来源表记录ID，对应BaiduPoiHerbBase.Id。
    /// </summary>
    public long? SourceId { get; private set; }

    /// <summary>
    /// 药材基地处理状态，例如PENDING、FOLLOWING、DEAL、LOST。
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
    public virtual CrmHerbBase? ParentHerbBase { get; private set; }

    /// <summary>
    /// 下级客户集合。
    /// </summary>
    public virtual ICollection<CrmHerbBase> Children { get; private set; } = new List<CrmHerbBase>();

    /// <summary>
    /// 客户跟进记录集合。
    /// </summary>
    public virtual ICollection<CrmFollowRecord> FollowRecords { get; private set; } = new List<CrmFollowRecord>();

    private CrmHerbBase() { }

    private CrmHerbBase(
        string herbBaseName,
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
        long? sourceId,
        Guid? ownerUserId,
        string remark,
        Guid? parentId,
        string subjectName)
    {
        BaseName = herbBaseName;
        SubjectName = subjectName;
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
        SourceId = sourceId;
        OwnerUserId = ownerUserId;
        Remark = remark;
        ParentId = parentId;
        Status = PendingContactStatus;
    }

    public static CrmHerbBase Create(
        string herbBaseName,
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
        long? sourceId,
        Guid? ownerUserId,
        string remark,
        Guid? parentId = null,
        string subjectName = "")
    {
        return new CrmHerbBase(
            herbBaseName,
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
            sourceId,
            ownerUserId,
            remark,
            parentId,
            subjectName);
    }

    public void UpdateBasicInfo(
        string herbBaseName,
        string mainProduct,
        string grade,
        int score,
        string province,
        string city,
        string area,
        string address,
        decimal? lat,
        decimal? lng,
        string remark,
        string subjectName = "")
    {
        BaseName = herbBaseName;
        SubjectName = subjectName;
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

    public void SetParent(Guid? parentId)
    {
        ParentId = parentId;
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

    public void UpdateSource(string sourcePlatform, long? sourceId)
    {
        SourcePlatform = sourcePlatform;
        SourceId = sourceId;
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



