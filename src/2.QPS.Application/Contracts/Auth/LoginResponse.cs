namespace QPS.Application.Contracts.Auth;

public class LoginResponse
{
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string RealName { get; set; }
    public string Role { get; set; }
    public Guid MerchantId { get; set; }
}