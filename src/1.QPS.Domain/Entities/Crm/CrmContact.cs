using QPS.Domain.Common;
using QPS.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace QPS.Domain.Entities.Crm;

public class CrmContact : BaseEntity
{
    private static readonly Regex MobilePhoneRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
    private static readonly Regex LandlinePhoneRegex = new(@"^0\d{2,3}-?\d{7,8}(-\d{1,6})?$", RegexOptions.Compiled);

    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PhoneType { get; private set; } = string.Empty;
    public string Wechat { get; private set; } = string.Empty;
    public string RoleName { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string Remark { get; private set; } = string.Empty;
    public virtual ICollection<CrmFollowRecord> FollowRecords { get; private set; } = new List<CrmFollowRecord>();

    private CrmContact() { }

    private CrmContact(
        string entityType,
        Guid entityId,
        string contactName,
        string phone,
        string phoneType,
        string wechat,
        string roleName,
        bool isPrimary,
        string remark)
    {
        EntityType = Trim(entityType);
        EntityId = entityId;
        ContactName = Trim(contactName);
        Phone = Trim(phone);
        PhoneType = Trim(phoneType);
        Wechat = Trim(wechat);
        RoleName = Trim(roleName);
        IsPrimary = isPrimary;
        Remark = Trim(remark);
        Status = "UNVERIFIED";
    }

    public static CrmContact Create(
        string entityType,
        Guid entityId,
        string contactName,
        string phone,
        string phoneType,
        string wechat,
        string roleName,
        bool isPrimary,
        string remark)
    {
        EnsureContactNameOrPhone(contactName, phone);

        return new CrmContact(entityType, entityId, contactName, phone, phoneType, wechat, roleName, isPrimary, remark);
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
        EnsureContactNameOrPhone(contactName, phone);

        ContactName = Trim(contactName);
        Phone = Trim(phone);
        PhoneType = Trim(phoneType);
        Wechat = Trim(wechat);
        RoleName = Trim(roleName);
        IsPrimary = isPrimary;
        Remark = Trim(remark);
    }

    private static void EnsureContactNameOrPhone(string contactName, string phone)
    {
        if (string.IsNullOrWhiteSpace(contactName) && string.IsNullOrWhiteSpace(phone))
        {
            throw new BusinessException(400, "联系人姓名和电话至少填写一项");
        }

        if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhone(Trim(phone)))
        {
            throw new BusinessException(400, "联系电话格式不正确");
        }
    }

    private static string Trim(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static bool IsValidPhone(string phone)
    {
        return MobilePhoneRegex.IsMatch(phone) || LandlinePhoneRegex.IsMatch(phone);
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
