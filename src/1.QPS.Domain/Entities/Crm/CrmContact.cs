using QPS.Domain.Common;
using QPS.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace QPS.Domain.Entities.Crm;

/// <summary>
/// CRM通用业务实体联系人。
/// </summary>
public class CrmContact : BaseEntity
{
    private static readonly Regex MobilePhoneRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
    private static readonly Regex LandlinePhoneRegex = new(@"^0\d{2,3}-?\d{7,8}(-\d{1,6})?$", RegexOptions.Compiled);

    /// <summary>
    /// 所属业务实体类型，例如CRM_HERB_BASE、CRM_VENDOR。
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>
    /// 所属业务实体ID。
    /// </summary>
    public Guid EntityId { get; private set; }

    /// <summary>
    /// 联系人姓名。
    /// </summary>
    public string ContactName { get; private set; } = string.Empty;

    /// <summary>
    /// 联系电话，导入CRM和后续跟进使用。
    /// </summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>
    /// 电话类型，例如MOBILE、LANDLINE、UNKNOWN。
    /// </summary>
    public string PhoneType { get; private set; } = string.Empty;

    /// <summary>
    /// 微信号。
    /// </summary>
    public string Wechat { get; private set; } = string.Empty;

    /// <summary>
    /// 联系人角色，例如负责人、采购、种植户。
    /// </summary>
    public string RoleName { get; private set; } = string.Empty;

    /// <summary>
    /// 是否主联系人。
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// 联系人状态，例如UNVERIFIED、VALID、INVALID。
    /// </summary>
    public string Status { get; private set; } = string.Empty;

    /// <summary>
    /// 备注，例如电话需二次确认、无人接听。
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>
    /// 联系人关联的跟进记录集合。
    /// </summary>
    public virtual ICollection<CrmFollowRecord> FollowRecords { get; private set; } = new List<CrmFollowRecord>();

    /// <summary>
    /// EF Core 使用的无参构造函数。
    /// </summary>
    private CrmContact() { }

    /// <summary>
    /// 初始化联系人实体。
    /// </summary>
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

    /// <summary>
    /// 创建联系人。
    /// </summary>
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

    /// <summary>
    /// 更新联系人基础信息。
    /// </summary>
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

    /// <summary>
    /// 校验联系人姓名和电话不能同时为空。
    /// </summary>
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

    /// <summary>
    /// 标记为主联系人。
    /// </summary>
    public void MarkPrimary()
    {
        IsPrimary = true;
    }

    /// <summary>
    /// 取消主联系人标记。
    /// </summary>
    public void UnmarkPrimary()
    {
        IsPrimary = false;
    }

    /// <summary>
    /// 更新联系人状态和备注。
    /// </summary>
    public void MarkStatus(string status, string remark)
    {
        Status = status;
        Remark = remark;
    }
}


