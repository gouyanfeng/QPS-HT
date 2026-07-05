namespace QPS.Application.Contracts.Permissions;

public class PermissionCreateRequest
{
    public string PermissionCode { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
}
