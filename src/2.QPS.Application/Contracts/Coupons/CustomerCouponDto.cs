namespace QPS.Application.Contracts.Coupons;

public class CustomerCouponDto
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public string CouponTitle { get; set; }
    public decimal CouponValue { get; set; }
    public decimal CouponMinConsume { get; set; }
    public DateTime CouponValidTo { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerPhone { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}