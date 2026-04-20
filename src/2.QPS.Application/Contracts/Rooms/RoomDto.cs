namespace QPS.Application.Contracts.Rooms;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; }
    public string Status { get; set; }
    public string MqttTopic { get; set; }
}