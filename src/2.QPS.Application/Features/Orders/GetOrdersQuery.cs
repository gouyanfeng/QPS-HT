using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Orders;

public class GetOrdersQuery : PaginationRequest, IRequest<PaginationResponse<OrderDto>>
{
    public string? OrderNo { get; set; }
    public string? Status { get; set; }
    public Guid? ShopId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PaginationResponse<OrderDto>>
{
    private readonly IDbContext _dbContext;

    public GetOrdersHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = from o in _dbContext.Orders
                    join s in _dbContext.Shops on o.ShopId equals s.Id into shopJoin
                    from shop in shopJoin.DefaultIfEmpty()
                    join r in _dbContext.Rooms on o.RoomId equals r.Id into roomJoin
                    from room in roomJoin.DefaultIfEmpty()
                    join c in _dbContext.Customers on o.CustomerId equals c.Id into customerJoin
                    from customer in customerJoin.DefaultIfEmpty()
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

        if (!string.IsNullOrEmpty(request.OrderNo))
        {
            query = query.Where(o => o.OrderNo.Contains(request.OrderNo));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(o => o.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (request.ShopId.HasValue)
        {
            query = query.Where(o => o.ShopId == request.ShopId.Value);
        }

        if (request.RoomId.HasValue)
        {
            query = query.Where(o => o.RoomId == request.RoomId.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

        return await query.ToPaginationResponseAsync(request);
    }
}