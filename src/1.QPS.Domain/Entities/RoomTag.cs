using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class RoomTag : BaseEntity
{
    public Guid RoomId { get; private set; }
    public Guid TagId { get; private set; }

    public Room Room { get; private set; }
    public Tag Tag { get; private set; }

    protected RoomTag() { }

    public RoomTag(Guid roomId, Guid tagId)
    {
        RoomId = roomId;
        TagId = tagId;
    }
}