namespace QPS.Application.Contracts.Crm;

public class CrmTransferRecordDto
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public Guid? FromOwnerUserId { get; set; }

    public string FromOwnerUserName { get; set; } = string.Empty;

    public Guid? ToOwnerUserId { get; set; }

    public string ToOwnerUserName { get; set; } = string.Empty;

    public Guid? OperatorUserId { get; set; }

    public string OperatorUserName { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}




