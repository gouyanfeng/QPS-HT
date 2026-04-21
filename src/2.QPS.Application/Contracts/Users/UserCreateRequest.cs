namespace QPS.Application.Contracts.Users;

public class UserCreateRequest
{
    public Guid MerchantId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RealName { get; set; }
}