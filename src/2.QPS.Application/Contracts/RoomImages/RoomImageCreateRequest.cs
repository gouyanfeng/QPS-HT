namespace QPS.Application.Contracts.RoomImages;

public class RoomImageCreateRequest
{
    public Guid RoomId { get; set; }
    public string ImageUrl { get; set; }
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}