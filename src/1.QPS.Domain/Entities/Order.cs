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
        Status = OrderStatus.WaitingPayment;
        StartTime = DateTime.UtcNow;
        OriginAmount = 0;
        DiscountAmount = 0;
        ActualAmount = 0;
    }

    public static Order Create(Guid shopId, Guid roomId, Guid? customerId)
    {
        var orderNo = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + new Random().Next(100000, 999999).ToString();
        return new Order(orderNo, shopId, roomId, customerId);
    }

    public void Start() { Status = OrderStatus.WaitingPayment; }
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
    public void Pay()
    {
        if (Status != OrderStatus.WaitingPayment) throw new InvalidOperationException("只能支付待支付的订单");
        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }
    public void Refund()
    {
        if (Status != OrderStatus.Paid) throw new InvalidOperationException("只能退款已支付的订单");
        Status = OrderStatus.Refunded;
        // 全额退款，不支持部分
    }
    public void Timeout()
    {
        if (Status != OrderStatus.WaitingPayment) throw new InvalidOperationException("只能超时期待支付的订单");
        Status = OrderStatus.Timeout;
    }
    public void Cancel() { Status = OrderStatus.Cancelled; }

    /// <summary>
    /// 设置订单有效时长
    /// </summary>
    /// <summary>
    /// 自动完成订单（用于定时任务，不涉及金额）
    /// </summary>
    public void Complete()
    {
        if (Status != OrderStatus.Paid) throw new InvalidOperationException("只能完成已支付的订单");
        Status = OrderStatus.Completed;
        EndTime = DateTime.UtcNow;
    }

    public void SetTimeLimit(DateTime endTime) => EndTime = endTime;

    /// <summary>
    /// 分配支付金额和方式
    /// </summary>
    public void AssignPaymentDetails(decimal originAmount, decimal actualAmount, string paymentMethod)
    {
        OriginAmount = originAmount;
        ActualAmount = actualAmount;
        PaymentMethod = paymentMethod;
    }
}