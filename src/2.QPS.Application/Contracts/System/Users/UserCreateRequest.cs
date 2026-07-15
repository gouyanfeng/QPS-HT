namespace QPS.Application.Contracts.System.Users;

public class UserCreateRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string RealName { get; set; }
    public Guid RoleId { get; set; }
}