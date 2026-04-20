using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.PricingAggregate;

public class Discount : Entity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    protected Discount() { }

    public Discount(Guid merchantId, string name, decimal rate, DateTime startTime, DateTime endTime)
    {
        MerchantId = merchantId;
        Name = name;
        Rate = rate;
        StartTime = startTime;
        EndTime = endTime;
    }

    public void Update(string name, decimal rate, DateTime startTime, DateTime endTime)
    {
        Name = name;
        Rate = rate;
        StartTime = startTime;
        EndTime = endTime;
    }
}