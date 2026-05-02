using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Title { get; private set; }
    public string CouponType { get; private set; }
    public decimal Value { get; private set; }
    public decimal MinConsume { get; private set; }
    public DateTime ValidTo { get; private set; }

    protected Coupon() { }

    public Coupon(string title, string couponType, decimal value, decimal minConsume, DateTime validTo)
    {
        Title = title;
        CouponType = couponType;
        Value = value;
        MinConsume = minConsume;
        ValidTo = validTo;
    }

    public void Update(string title, string couponType, decimal value, decimal minConsume, DateTime validTo)
    {
        Title = title;
        CouponType = couponType;
        Value = value;
        MinConsume = minConsume;
        ValidTo = validTo;
    }
}