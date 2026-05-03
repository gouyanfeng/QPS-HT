using MediatR;
using QPS.Application.Contracts.Orders;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Orders;

public class GetOrdersQuery : PaginationRequest, IRequest<PaginationResponse<OrderDto>>
{
    public string? OrderNo { get; set; }
    public OrderStatus? Status { get; set; }
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
        IQueryable<Order> query = _dbContext.Orders.AsNoTracking();

        if (!string.IsNullOrEmpty(request.OrderNo))
        {
            query = query.Where(o => o.OrderNo.Contains(request.OrderNo));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
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

        var dtoQuery = query
            .Include(o => o.Shop)
            .Include(o => o.Room)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                ShopId = o.ShopId,
                ShopName = o.Shop != null ? o.Shop.Name : null,
                RoomId = o.RoomId,
                RoomNumber = o.Room != null ? o.Room.Name : null,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.Nickname : null,
                Status = o.Status.ToChinese(),
                OriginAmount = o.OriginAmount,
                DiscountAmount = o.DiscountAmount,
                ActualAmount = o.ActualAmount,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                PaymentMethod = o.PaymentMethod,
                PaidAt = o.PaidAt,
                CreatedAt = o.CreatedAt,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ItemName = oi.ItemName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    Amount = oi.Amount
                }).ToList()
            });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}