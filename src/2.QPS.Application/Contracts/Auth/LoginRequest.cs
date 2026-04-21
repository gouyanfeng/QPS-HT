namespace QPS.Application.Contracts.Auth;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public Guid MerchantId { get; set; }
}