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
            OrderStatus.Pending => "待处理",
            OrderStatus.Active => "进行中",
            OrderStatus.Completed => "已完成",
            OrderStatus.Cancelled => "已取消",
            _ => status.ToString()
        };
    }
}