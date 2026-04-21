using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Orders;

/// <summary>
/// 结算订单命令
/// </summary>
public class SettleOrderCommand : IRequest<bool>
{
    /// <summary>
    /// 结算订单请求
    /// </summary>
    public SettleOrderRequest Request { get; set; }
}

/// <summary>
/// 结算订单处理器
/// </summary>
public class SettleOrderHandler : IRequestHandler<SettleOrderCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMqttService _mqttService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="currentUserService">当前用户服务</param>
    /// <param name="mqttService">MQTT服务</param>
    public SettleOrderHandler(IDbContext dbContext, ICurrentUserService currentUserService, IMqttService mqttService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _mqttService = mqttService;
    }

    /// <summary>
    /// 处理结算订单请求
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    public async Task<bool> Handle(SettleOrderCommand request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;
        var order = await _dbContext.Orders.FindAsync(request.Request.OrderId, cancellationToken);
        if (order == null || order.MerchantId != merchantId) return false;

        // 完成订单
        order.Complete(0, 0, 0, "CASH"); // 示例值，实际应从请求中获取

        // 获取对应的房间
        var room = await _dbContext.Rooms.FindAsync(order.RoomId, cancellationToken);
        if (room != null)
        {
            // 更新房间状态为空闲
            room.SetToIdle();

            // 发送MQTT命令关闭设备
            await _mqttService.SendCommandAsync(room.MqttTopic, "POWER_OFF");
        }

        // 保存到数据库
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}