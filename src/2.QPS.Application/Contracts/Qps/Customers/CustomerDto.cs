namespace QPS.Application.Contracts.Qps.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string OpenId { get; set; }
    public string Phone { get; set; }
    public string Nickname { get; set; }
    public string AvatarUrl { get; set; }
}