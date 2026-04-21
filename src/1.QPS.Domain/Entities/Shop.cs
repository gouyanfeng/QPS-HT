using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Shop : BaseEntity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }
    public int AutoPowerOffDelay { get; private set; } // 自动关机延迟（分钟）

    protected Shop() { }

    private Shop(string name, string address, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        MerchantId = Guid.Empty;
        Name = name;
        Address = address;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        AutoPowerOffDelay = autoPowerOffDelay;
    }

    public static Shop Create(string name, string address, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        return new Shop(name, address, openingTime, closingTime, autoPowerOffDelay);
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