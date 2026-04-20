using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.RoomAggregate;

public class Room : AggregateRoot
{
    public string RoomNumber { get; private set; }
    public RoomStatus Status { get; private set; }
    public DeviceConfig DeviceConfig { get; private set; }
    public Guid MerchantId { get; private set; }

    protected Room() { }

    public Room(string roomNumber, DeviceConfig deviceConfig, Guid merchantId)
    {
        RoomNumber = roomNumber;
        Status = RoomStatus.Idle;
        DeviceConfig = deviceConfig;
        MerchantId = merchantId;
    }

    public void Occupy() { Status = RoomStatus.Occupied; }
    public void Clean() { Status = RoomStatus.Cleaning; }
    public void MarkAsFault() { Status = RoomStatus.Fault; }
    public void SetToIdle() { Status = RoomStatus.Idle; }
}