using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Room : AggregateRoot
{
    public Guid MerchantId { get; private set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; }
    public RoomStatus Status { get; private set; }
    public decimal UnitPrice { get; private set; } // 单价
    public bool IsEnabled { get; private set; } // 是否启用

    private Room(Guid shopId, string name, decimal unitPrice, bool isEnabled = true)
    {
        MerchantId = Guid.Empty;
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