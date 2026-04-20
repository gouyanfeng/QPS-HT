using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.OrderAggregate;
using QPS.Domain.Aggregates.RoomAggregate;
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
    private readonly ITenantService _tenantService;
    private readonly IMqttService _mqttService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="tenantService">租户服务</param>
    /// <param name="mqttService">MQTT服务</param>
    public CreateOrderHandler(IDbContext dbContext, ITenantService tenantService, IMqttService mqttService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
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
        var tenantId = _tenantService.GetCurrentTenantId();
        var room = await _dbContext.Rooms.FindAsync(request.Request.RoomId, cancellationToken);
        if (room == null)
        {
            throw new DomainException("房间不存在");
        }
        if (room.MerchantId != tenantId)
        {
            throw new DomainException("无权操作此房间");
        }

        // 检查房间是否可用
        if (room.Status != RoomStatus.Idle)
        {
            throw new DomainException("房间当前不可用");
        }

        // 生成订单号
        var orderNumber = GenerateOrderNumber();
        
        // 创建订单
        var order = new Order(orderNumber, room.MerchantId, room.ShopId, room.Id, null);
        order.Start();

        // 更新房间状态为占用
        room.Occupy();

        // 发送MQTT命令开启设备
        await _mqttService.SendCommandAsync(room.MqttTopic, "POWER_ON");

        // 保存到数据库
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    /// <summary>
    /// 生成订单号
    /// </summary>
    /// <returns>订单号</returns>
    private string GenerateOrderNumber()
    {
        return $"ORD{DateTime.Now:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N").Substring(0, 4)}";
    }
}