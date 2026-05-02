using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Discount : BaseEntity
{
    public string Name { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    protected Discount() { }

    public Discount(string name, decimal rate, DateTime startTime, DateTime endTime)
    {
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