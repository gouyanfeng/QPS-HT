namespace QPS.Application.Contracts.Coupons;

public class CouponDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string CouponType { get; set; }
    public decimal Value { get; set; }
    public decimal MinConsume { get; set; }
    public DateTime ValidTo { get; set; }
}