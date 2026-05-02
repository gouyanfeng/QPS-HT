using QPS.Application.Contracts.RoomImages;
using QPS.Application.Contracts.Tags;

namespace QPS.Application.Contracts.Shops;

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