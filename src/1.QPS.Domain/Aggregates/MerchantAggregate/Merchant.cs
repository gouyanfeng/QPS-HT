using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.MerchantAggregate;

public class Merchant : AggregateRoot
{
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Merchant() { }

    public Merchant(string name, string phone, DateTime? expiryDate)
    {
        Name = name;
        Phone = phone;
        ExpiryDate = expiryDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string phone, DateTime? expiryDate)
    {
        Name = name;
        Phone = phone;
        ExpiryDate = expiryDate;
    }

    public void Activate() { IsActive = true; }
    public void Deactivate() { IsActive = false; }
}