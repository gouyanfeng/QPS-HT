namespace QPS.Application.Contracts.Roles;

public class RoleDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}