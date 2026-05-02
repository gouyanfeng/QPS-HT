using QPS.Domain.Common;

namespace QPS.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNo { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    public virtual Shop Shop { get; set; }
    public virtual Room Room { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public decimal OriginAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ActualAmount { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public string? PaymentMethod { get; private set; }
    public DateTime? PaidAt { get; private set; }

    protected Order() { }

    public Order(string orderNo, Guid shopId, Guid roomId, Guid? customerId)
    {
        OrderNo = orderNo;
        ShopId = shopId;
        RoomId = roomId;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        StartTime = DateTime.UtcNow;
        OriginAmount = 0;
        DiscountAmount = 0;
        ActualAmount = 0;
    }

    public static Order Create(Guid shopId, Guid roomId, Guid? customerId)
    {
        var orderNo = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100000, 999999).ToString();
        return new Order(orderNo, shopId, roomId, customerId);
    }

    public void Start() { Status = OrderStatus.Active; }
    public void Complete(decimal originAmount, decimal discountAmount, decimal actualAmount, string paymentMethod)
    {
        Status = OrderStatus.Completed;
        EndTime = DateTime.UtcNow;
        OriginAmount = originAmount;
        DiscountAmount = discountAmount;
        ActualAmount = actualAmount;
        PaymentMethod = paymentMethod;
        PaidAt = DateTime.UtcNow;
    }
    public void Cancel() { Status = OrderStatus.Cancelled; }
}