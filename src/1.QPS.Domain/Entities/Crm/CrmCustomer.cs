using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

public class CrmCustomer : BaseEntity
{
    public Guid? ParentCustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerType { get; private set; } = string.Empty;
    public string MainProduct { get; private set; } = string.Empty;
    public string Grade { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public string Province { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Area { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public decimal? Lat { get; private set; }
    public decimal? Lng { get; private set; }
    public string SourcePlatform { get; private set; } = string.Empty;
    public long? SourceLeadId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public Guid? OwnerUserId { get; private set; }
    public string Remark { get; private set; } = string.Empty;

    public virtual CrmCustomer? ParentCustomer { get; private set; }
    public virtual ICollection<CrmCustomer> Children { get; private set; } = new List<CrmCustomer>();
    public virtual ICollection<CrmContact> Contacts { get; private set; } = new List<CrmContact>();
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
        Status = "待联系";
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

    public void UpdateStatus(string status, string remark)
    {
        Status = status;
        Remark = remark;
    }
}
