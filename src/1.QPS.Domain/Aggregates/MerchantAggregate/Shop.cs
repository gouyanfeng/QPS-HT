using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.MerchantAggregate;

public class Shop : Entity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }
    public int AutoPowerOffDelay { get; private set; } // 自动关机延迟（分钟）

    protected Shop() { }

    public Shop(Guid merchantId, string name, string address, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        MerchantId = merchantId;
        Name = name;
        Address = address;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        AutoPowerOffDelay = autoPowerOffDelay;
    }

    public void Update(string name, string address, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        Name = name;
        Address = address;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        AutoPowerOffDelay = autoPowerOffDelay;
    }
}