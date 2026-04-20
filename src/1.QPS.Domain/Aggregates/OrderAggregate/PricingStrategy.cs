using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.OrderAggregate;

public class PricingStrategy : ValueObject
{
    public decimal BasePricePerHour { get; private set; }
    public decimal StepPricePerHour { get; private set; }
    public int StepHours { get; private set; }
    public decimal DailyCap { get; private set; }

    protected PricingStrategy() { }

    public PricingStrategy(decimal basePricePerHour, decimal stepPricePerHour, int stepHours, decimal dailyCap)
    {
        BasePricePerHour = basePricePerHour;
        StepPricePerHour = stepPricePerHour;
        StepHours = stepHours;
        DailyCap = dailyCap;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BasePricePerHour;
        yield return StepPricePerHour;
        yield return StepHours;
        yield return DailyCap;
    }
}