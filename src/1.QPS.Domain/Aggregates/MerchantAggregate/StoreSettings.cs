using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.MerchantAggregate;

public class StoreSettings : ValueObject
{
    public int PowerOffDelayMinutes { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }

    protected StoreSettings() { }

    public StoreSettings(int powerOffDelayMinutes, TimeSpan openingTime, TimeSpan closingTime)
    {
        PowerOffDelayMinutes = powerOffDelayMinutes;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PowerOffDelayMinutes;
        yield return OpeningTime;
        yield return ClosingTime;
    }
}