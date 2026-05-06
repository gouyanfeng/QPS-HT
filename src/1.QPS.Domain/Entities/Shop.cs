using QPS.Domain.Common;
using System.Collections.Generic;

namespace QPS.Domain.Entities;

public class Shop : BaseEntity
{
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string Phone { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }
    public int AutoPowerOffDelay { get; private set; }

    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    protected Shop() { }

    private Shop(string name, string address, string phone, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        Name = name;
        Address = address;
        Phone = phone;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        AutoPowerOffDelay = autoPowerOffDelay;
    }

    public static Shop Create(string name, string address, string phone, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        return new Shop(name, address, phone, openingTime, closingTime, autoPowerOffDelay);
    }

    public void Update(string name, string address, string phone, TimeSpan openingTime, TimeSpan closingTime, int autoPowerOffDelay)
    {
        Name = name;
        Address = address;
        Phone = phone;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        AutoPowerOffDelay = autoPowerOffDelay;
    }
}