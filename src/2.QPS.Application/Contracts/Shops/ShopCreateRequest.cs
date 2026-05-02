namespace QPS.Application.Contracts.Shops;

public class ShopCreateRequest
{
    public string Name { get; set; }
    public string Address { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public int AutoPowerOffDelay { get; set; }
}