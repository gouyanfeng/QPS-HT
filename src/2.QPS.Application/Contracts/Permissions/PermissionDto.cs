namespace QPS.Application.Contracts.Permissions;

public class PermissionDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string PermissionCode { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
}
