using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Orders;

public class GetOrderQuery : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
}

public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IDbContext _dbContext;

    public GetOrderHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Include(o => o.Shop)
            .Include(o => o.Room)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new BusinessException(404, "订单不存在");
        }

        return new OrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            ShopId = order.ShopId,
            ShopName = order.Shop != null ? order.Shop.Name : null,
            RoomId = order.RoomId,
            RoomNumber = order.Room != null ? order.Room.Name : null,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer != null ? order.Customer.Nickname : null,
            Status = order.Status.ToString(),
            OriginAmount = order.OriginAmount,
            DiscountAmount = order.DiscountAmount,
            ActualAmount = order.ActualAmount,
            StartTime = order.StartTime,
            EndTime = order.EndTime,
            PaymentMethod = order.PaymentMethod,
            PaidAt = order.PaidAt,
            CreatedAt = order.CreatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ItemName = oi.ItemName,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                Amount = oi.Amount
            }).ToList()
        };
    }
}