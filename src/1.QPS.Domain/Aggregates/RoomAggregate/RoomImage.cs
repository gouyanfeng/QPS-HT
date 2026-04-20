using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.RoomAggregate;

public class RoomImage : Entity
{
    public Guid RoomId { get; private set; }
    public string ImageUrl { get; private set; }
    public bool IsMain { get; private set; }
    public int SortOrder { get; private set; }

    protected RoomImage() { }

    public RoomImage(Guid roomId, string imageUrl, bool isMain, int sortOrder)
    {
        RoomId = roomId;
        ImageUrl = imageUrl;
        IsMain = isMain;
        SortOrder = sortOrder;
    }

    public void Update(string imageUrl, bool isMain, int sortOrder)
    {
        ImageUrl = imageUrl;
        IsMain = isMain;
        SortOrder = sortOrder;
    }
}