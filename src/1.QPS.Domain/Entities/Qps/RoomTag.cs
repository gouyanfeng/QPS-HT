using QPS.Domain.Common;

namespace QPS.Domain.Entities.Qps;

public class RoomTag : BaseEntity
{
    public Guid RoomId { get; private set; }
    public Guid TagId { get; private set; }

    public Room Room { get; private set; }
    public Tag Tag { get; private set; }

    protected RoomTag() { }

    public RoomTag(Guid roomId, Guid tagId, Guid merchantId)
    {
        RoomId = roomId;
        TagId = tagId;
        MerchantId = merchantId;
    }
}