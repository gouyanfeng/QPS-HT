namespace QPS.Application.Contracts.Crm;

public class CrmContactStatusRequest
{
    public string Status { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;
}
