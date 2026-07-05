namespace QPS.Application.Contracts.Permissions;

public class PermissionUpdateRequest
{
    public string PermissionCode { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
}
