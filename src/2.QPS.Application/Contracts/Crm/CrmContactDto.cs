namespace QPS.Application.Contracts.Crm;

public class CrmContactDto
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string ContactName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PhoneType { get; set; } = string.Empty;

    public string Wechat { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


