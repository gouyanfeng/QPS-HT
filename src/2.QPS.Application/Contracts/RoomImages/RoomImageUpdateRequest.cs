namespace QPS.Application.Contracts.RoomImages;

public class RoomImageUpdateRequest
{
    public string ImageUrl { get; set; }
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}