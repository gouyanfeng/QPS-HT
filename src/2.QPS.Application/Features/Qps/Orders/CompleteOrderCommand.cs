using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;using QPS.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Qps.Orders;

public class CompleteOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public decimal OriginAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public string PaymentMethod { get; set; }
}

public class CompleteOrderHandler : IRequestHandler<CompleteOrderCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public CompleteOrderHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync(request.OrderId, cancellationToken);
        if (order == null) return false;

        // 完成订单
        order.Complete(request.OriginAmount, request.DiscountAmount, request.ActualAmount, request.PaymentMethod);

        // 获取对应的房间
        var room = await _dbContext.Rooms.FindAsync(order.RoomId, cancellationToken);
        if (room != null)
        {
            // 更新房间状态为空闲
            room.SetToIdle();
        }

        // 保存到数据库
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 发布订单完成事件（用于自动断电）
        await _publisher.Publish(new OrderCompletedEvent(order.Id, order.RoomId), cancellationToken);

        // 发布会话过期事件（用于会话级别处理）
        await _publisher.Publish(new SessionExpiredEvent(order.Id), cancellationToken);

        return true;
    }
}