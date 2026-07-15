using QPS.Application.Contracts.Qps.RoomImages;
using QPS.Application.Contracts.Qps.Tags;

namespace QPS.Application.Contracts.Qps.Shops;

public class RoomSummaryDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Name { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsAvailable { get; set; }
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
    public List<RoomImageDto> Images { get; set; } = new List<RoomImageDto>();
}