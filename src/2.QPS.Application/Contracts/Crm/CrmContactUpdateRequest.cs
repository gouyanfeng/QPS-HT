namespace QPS.Application.Contracts.Crm;

public class CrmContactUpdateRequest
{
    public string ContactName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PhoneType { get; set; } = "UNKNOWN";

    public string Wechat { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public string Remark { get; set; } = string.Empty;
}


