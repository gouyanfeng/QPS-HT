using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Orders;

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
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public CompleteOrderHandler(IDbContext dbContext, ICurrentUserService currentUserService, IPublisher publisher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;
        var order = await _dbContext.Orders.FindAsync(request.OrderId, cancellationToken);
        if (order == null || order.MerchantId != merchantId) return false;

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

        return true;
    }
}