namespace QPS.Application.Contracts.Shops;

public class ShopDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public int AutoPowerOffDelay { get; set; }
    public List<RoomSummaryDto> Rooms { get; set; } = new List<RoomSummaryDto>();
}