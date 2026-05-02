namespace QPS.Application.Contracts.Customers;

public class CustomerCreateRequest
{
    public string OpenId { get; set; }
    public string Phone { get; set; }
    public string Nickname { get; set; }
    public string AvatarUrl { get; set; }
}