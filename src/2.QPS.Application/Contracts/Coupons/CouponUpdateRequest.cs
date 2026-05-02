namespace QPS.Application.Contracts.Coupons;

public class CouponUpdateRequest
{
    public string Title { get; set; }
    public string CouponType { get; set; }
    public decimal Value { get; set; }
    public decimal MinConsume { get; set; }
    public DateTime ValidTo { get; set; }
}