using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class RoomPlan : BaseEntity
{
    public Guid RoomId { get; private set; }
    public Guid PlanId { get; private set; }

    public virtual Room Room { get; private set; }
    public virtual Plan Plan { get; private set; }

    protected RoomPlan() { }

    public RoomPlan(Guid roomId, Guid planId, Guid merchantId)
    {
        RoomId = roomId;
        PlanId = planId;
        MerchantId = merchantId;
    }
}