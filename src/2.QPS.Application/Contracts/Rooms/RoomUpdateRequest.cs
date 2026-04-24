namespace QPS.Application.Contracts.Rooms;

public class RoomUpdateRequest
{
    /// <summary>
    /// 房间号
    /// </summary>
    public string RoomNumber { get; set; }

    /// <summary>
    /// 店铺ID
    /// </summary>
    public Guid ShopId { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}