using QPS.Domain.Common;

namespace QPS.Domain.Aggregates.RoomAggregate;

public class RoomTagMapping : Entity
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