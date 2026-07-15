namespace QPS.Application.Contracts.System.Users;

public class UserUpdateRequest
{
    public string RealName { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
}