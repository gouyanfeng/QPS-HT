using QPS.Domain.Entities;

namespace QPS.Application.Extensions;

public static class RoomStatusExtensions
{
    public static string ToChinese(this RoomStatus status)
    {
        return status switch
        {
            RoomStatus.Idle => "空闲",
            RoomStatus.Occupied => "占用",
            RoomStatus.Cleaning => "清洁中",
            RoomStatus.Fault => "故障",
            _ => status.ToString()
        };
    }
}

public static class OrderStatusExtensions
{
    public static string ToChinese(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Completed => "已完成",
            OrderStatus.WaitingPayment => "待支付",
            OrderStatus.Paid => "已支付",
            OrderStatus.Refunding => "退款中",
            OrderStatus.Refunded => "已退款",
            OrderStatus.Cancelled => "已取消",
            OrderStatus.Timeout => "已超时",
            _ => status.ToString()
        };
    }
}