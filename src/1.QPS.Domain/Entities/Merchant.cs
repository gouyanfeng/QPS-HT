using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Merchant : AggregateRoot
{
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Merchant() { }

    private Merchant(string name, string phone, DateTime? expiryDate)
    {
        Name = name;
        Phone = phone;
        ExpiryDate = expiryDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Merchant Create(string name, string phone, DateTime? expiryDate)
    {
        return new Merchant(name, phone, expiryDate);
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