using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.RoomAggregate;

public class Room : AggregateRoot
{
    public Guid MerchantId { get; private set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; }
    public RoomStatus Status { get; private set; }
    public string DeviceSn { get; private set; }
    public string MqttTopic { get; private set; }
    public decimal UnitPrice { get; private set; } // 单价

    protected Room() { }

    public Room(Guid merchantId, Guid shopId, string name, string deviceSn, string mqttTopic, decimal unitPrice)
    {
        MerchantId = merchantId;
        ShopId = shopId;
        Name = name;
        Status = RoomStatus.Idle;
        DeviceSn = deviceSn;
        MqttTopic = mqttTopic;
        UnitPrice = unitPrice;
    }

    public void Occupy() { Status = RoomStatus.Occupied; }
    public void Clean() { Status = RoomStatus.Cleaning; }
    public void MarkAsFault() { Status = RoomStatus.Fault; }
    public void SetToIdle() { Status = RoomStatus.Idle; }

    public void Update(string name, string deviceSn, string mqttTopic, decimal unitPrice)
    {
        Name = name;
        DeviceSn = deviceSn;
        MqttTopic = mqttTopic;
        UnitPrice = unitPrice;
    }
}