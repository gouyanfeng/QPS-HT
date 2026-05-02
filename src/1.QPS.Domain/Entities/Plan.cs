using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Plan : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; }

    protected Plan() { }

    public Plan(string name, string description, decimal price, int durationMinutes)
    {
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