using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.MerchantAggregate;

public class Merchant : AggregateRoot
{
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public StoreSettings StoreSettings { get; private set; }

    protected Merchant() { }

    public Merchant(string name, string phoneNumber, StoreSettings storeSettings)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        StoreSettings = storeSettings;
    }

    public void UpdateSettings(StoreSettings storeSettings)
    {
        StoreSettings = storeSettings;
    }
}