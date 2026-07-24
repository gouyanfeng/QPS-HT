using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

public class CrmContact : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PhoneType { get; private set; } = string.Empty;
    public string Wechat { get; private set; } = string.Empty;
    public string RoleName { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string Remark { get; private set; } = string.Empty;

    public virtual CrmCustomer? Customer { get; private set; }
    public virtual ICollection<CrmFollowRecord> FollowRecords { get; private set; } = new List<CrmFollowRecord>();

    private CrmContact() { }

    private CrmContact(
        Guid customerId,
        string contactName,
        string phone,
        string phoneType,
        string wechat,
        string roleName,
        bool isPrimary,
        string remark)
    {
        CustomerId = customerId;
        ContactName = contactName;
        Phone = phone;
        PhoneType = phoneType;
        Wechat = wechat;
        RoleName = roleName;
        IsPrimary = isPrimary;
        Remark = remark;
        Status = "未验证";
    }

    public static CrmContact Create(
        Guid customerId,
        string contactName,
        string phone,
        string phoneType,
        string wechat,
        string roleName,
        bool isPrimary,
        string remark)
    {
        return new CrmContact(customerId, contactName, phone, phoneType, wechat, roleName, isPrimary, remark);
    }

    public void Update(
        string contactName,
        string phone,
        string phoneType,
        string wechat,
        string roleName,
        bool isPrimary,
        string remark)
    {
        ContactName = contactName;
        Phone = phone;
        PhoneType = phoneType;
        Wechat = wechat;
        RoleName = roleName;
        IsPrimary = isPrimary;
        Remark = remark;
    }

    public void MarkPrimary()
    {
        IsPrimary = true;
    }

    public void UnmarkPrimary()
    {
        IsPrimary = false;
    }

    public void MarkStatus(string status, string remark)
    {
        Status = status;
        Remark = remark;
    }
}
