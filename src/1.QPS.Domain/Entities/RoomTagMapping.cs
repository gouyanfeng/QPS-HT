using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class RoomTagMapping : BaseEntity
{
    public Guid RoomId { get; private set; }
    public Guid TagId { get; private set; }

    protected RoomTagMapping() { }

    public RoomTagMapping(Guid roomId, Guid tagId)
    {
        RoomId = roomId;
        TagId = tagId;
    }
}