namespace QPS.Application.Contracts.Qps.Merchants;

public class MerchantCreateRequest
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public DateTime? ExpiryDate { get; set; }
}