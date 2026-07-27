namespace QPS.Application.Contracts.Crm;

public class CrmContactCreateRequest
{
    public string ContactName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PhoneType { get; set; } = "未知";

    public string Wechat { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public string Remark { get; set; } = string.Empty;
}
