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
        var query = from o in _dbContext.Orders
                    join s in _dbContext.Shops on o.ShopId equals s.Id into shopJoin
                    from shop in shopJoin.DefaultIfEmpty()
                    join r in _dbContext.Rooms on o.RoomId equals r.Id into roomJoin
                    from room in roomJoin.DefaultIfEmpty()
                    join c in _dbContext.Customers on o.CustomerId equals c.Id into customerJoin
                    from customer in customerJoin.DefaultIfEmpty()
                    where o.Id == request.OrderId
                    select new OrderDto
                    {
                        Id = o.Id,
                        OrderNo = o.OrderNo,
                        ShopId = o.ShopId,
                        ShopName = shop != null ? shop.Name : null,
                        RoomId = o.RoomId,
                        RoomNumber = room != null ? room.Name : null,
                        CustomerId = o.CustomerId,
                        CustomerName = customer != null ? customer.Nickname : null,
                        Status = o.Status.ToString(),
                        OriginAmount = o.OriginAmount,
                        DiscountAmount = o.DiscountAmount,
                        ActualAmount = o.ActualAmount,
                        StartTime = o.StartTime,
                        EndTime = o.EndTime,
                        PaymentMethod = o.PaymentMethod,
                        PaidAt = o.PaidAt,
                        CreatedAt = o.CreatedAt
                    };

        var order = await query.FirstOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            throw new BusinessException(404, "订单不存在");
        }

        return order;
    }
}