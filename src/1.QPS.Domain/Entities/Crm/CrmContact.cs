using QPS.Domain.Common;

namespace QPS.Domain.Entities.Crm;

/// <summary>
/// CRM通用业务实体联系人。
/// </summary>
public class CrmContact : BaseEntity
{
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
        EntityType = entityType;
        EntityId = entityId;
        ContactName = contactName;
        Phone = phone;
        PhoneType = phoneType;
        Wechat = wechat;
        RoleName = roleName;
        IsPrimary = isPrimary;
        Remark = remark;
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


