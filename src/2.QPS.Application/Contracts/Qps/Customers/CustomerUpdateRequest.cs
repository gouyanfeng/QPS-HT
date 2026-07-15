namespace QPS.Application.Contracts.Qps.Customers;

public class CustomerUpdateRequest
{
    public string Phone { get; set; }
    public string Nickname { get; set; }
    public string AvatarUrl { get; set; }
}