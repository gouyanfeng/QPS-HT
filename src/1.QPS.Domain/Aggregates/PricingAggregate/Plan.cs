using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.PricingAggregate;

public class Plan : AggregateRoot
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; } // 套餐时长（分钟）
    public bool IsActive { get; private set; }

    protected Plan() { }

    public Plan(Guid merchantId, string name, string description, decimal price, int durationMinutes)
    {
        MerchantId = merchantId;
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
        IsActive = true;
    }

    public void Update(string name, string description, decimal price, int durationMinutes)
    {
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
    }

    public void Activate() { IsActive = true; }
    public void Deactivate() { IsActive = false; }
}