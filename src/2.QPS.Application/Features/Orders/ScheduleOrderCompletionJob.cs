using MediatR;
using Microsoft.Extensions.Logging;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Events;
using System;
using System.Threading.Tasks;

namespace QPS.Application.Features.Orders;

public class ScheduleOrderCompletionJob
{
    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly ILogger<ScheduleOrderCompletionJob> _logger;

    public ScheduleOrderCompletionJob(IDbContext dbContext, IPublisher publisher, ILogger<ScheduleOrderCompletionJob> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Execute(Guid orderId)
    {
        try
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"订单 {orderId} 不存在");
                return;
            }

            // 检查订单状态，如果已经完成或取消，则不需要处理
            if (order.Status == OrderStatus.Completed ||
                order.Status == OrderStatus.Cancelled ||
                order.Status == OrderStatus.Refunded)
            {
                _logger.LogInformation($"订单 {order.OrderNo} 状态为 {order.Status}，无需自动完成");
                return;
            }

            // 如果订单还未支付，也不自动完成
            if (order.Status != OrderStatus.Paid)
            {
                _logger.LogInformation($"订单 {order.OrderNo} 状态为 {order.Status}，尚未支付");
                return;
            }

            // 完成订单
            order.GetType().GetProperty("Status")?.SetValue(order, (object)OrderStatus.Completed);

            // 获取对应的房间
            var room = await _dbContext.Rooms.FindAsync(order.RoomId);
            if (room != null)
            {
                room.SetToIdle();
            }

            // 保存到数据库
            await _dbContext.SaveChangesAsync();

            // 发布订单完成事件（用于自动断电）
            await _publisher.Publish(new OrderCompletedEvent(order.Id, order.RoomId));

            _logger.LogInformation($"订单 {order.OrderNo} 已自动完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"执行订单自动完成失败，订单ID: {orderId}");
        }
    }
}