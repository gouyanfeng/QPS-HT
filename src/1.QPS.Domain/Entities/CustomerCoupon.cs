using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class CustomerCoupon : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Status { get; private set; }

    protected CustomerCoupon() { }

    public CustomerCoupon(Guid couponId, Guid customerId, string status)
    {
        CouponId = couponId;
        CustomerId = customerId;
        Status = status;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
    }
}