namespace QPS.Application.Contracts.Merchants;

public class MerchantUpdateRequest
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}