using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Orders;

/// <summary>
/// 创建订单命令
/// </summary>
public class CreateOrderCommand : IRequest<Guid>
{
    /// <summary>
    /// 创建订单请求
    /// </summary>
    public CreateOrderRequest Request { get; set; }
}

/// <summary>
/// 创建订单处理器
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
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
    public CreateOrderHandler(IDbContext dbContext, ICurrentUserService currentUserService, IMqttService mqttService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _mqttService = mqttService;
    }

    /// <summary>
    /// 处理创建订单请求
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>订单ID</returns>
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.FindAsync(request.Request.RoomId, cancellationToken);
        if (room == null)
        {
            throw new BusinessException(404, "房间不存在");
        }
        // 检查房间是否可用
        if (room.Status != RoomStatus.Idle)
        {
            throw new BusinessException(400, "房间当前不可用");
        }

        // 创建订单
        var order = Order.Create(room.ShopId, room.Id, null);

        order.Start();

        // 更新房间状态为占用
        room.Occupy();

        // 由于移除了 MqttTopic 字段，不再发送 MQTT 命令
        // 可以根据实际需求添加其他逻辑

        // 保存到数据库
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}