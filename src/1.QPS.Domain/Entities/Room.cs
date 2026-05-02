using QPS.Domain.Common;
using System.Collections.Generic;

namespace QPS.Domain.Entities;

public class Room : AggregateRoot
{
    public Guid ShopId { get; private set; }
    public string Name { get; private set; }
    public RoomStatus Status { get; private set; }
    public decimal UnitPrice { get; private set; }
    public bool IsEnabled { get; private set; }

    public Shop Shop { get; private set; }
    public ICollection<RoomTag> RoomTags { get; private set; } = new List<RoomTag>();
    public ICollection<RoomImage> RoomImages { get; private set; } = new List<RoomImage>();
    public ICollection<RoomPlan> RoomPlans { get; private set; } = new List<RoomPlan>();

    private Room(Guid shopId, string name, decimal unitPrice, bool isEnabled = true)
    {
        ShopId = shopId;
        Name = name;
        Status = RoomStatus.Idle;
        UnitPrice = unitPrice;
        IsEnabled = isEnabled;
    }

    public static Room Create(Guid shopId, string name, decimal unitPrice, bool isEnabled = true)
    {
        return new Room(shopId, name, unitPrice, isEnabled);
    }

    public void Occupy() { Status = RoomStatus.Occupied; }
    public void Clean() { Status = RoomStatus.Cleaning; }
    public void MarkAsFault() { Status = RoomStatus.Fault; }
    public void SetToIdle() { Status = RoomStatus.Idle; }

    public void Update(string name, decimal unitPrice, bool isEnabled)
    {
        Name = name;
        UnitPrice = unitPrice;
        IsEnabled = isEnabled;
    }

    public void Enable() { IsEnabled = true; }
    public void Disable() { IsEnabled = false; }
}