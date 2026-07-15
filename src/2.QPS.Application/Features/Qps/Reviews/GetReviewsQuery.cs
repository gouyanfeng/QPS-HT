using MediatR;
using QPS.Application.Contracts.Qps.Reviews;
using QPS.Application.Interfaces;
using QPS.Application.Extensions;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Qps.Reviews;

public class GetReviewsQuery : PaginationRequest, IRequest<PaginationResponse<ReviewDto>>
{
    public Guid? RoomId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }
}

public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, PaginationResponse<ReviewDto>>
{
    private readonly IDbContext _dbContext;

    public GetReviewsQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Reviews.AsNoTracking();

        if (request.RoomId.HasValue)
        {
            query = query.Where(r => r.RoomId == request.RoomId.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(r => r.CustomerId == request.CustomerId.Value);
        }

        if (request.OrderId.HasValue)
        {
            query = query.Where(r => r.OrderId == request.OrderId.Value);
        }

        var dtoQuery = query.Select(r => new ReviewDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            RoomId = r.RoomId,
            CustomerId = r.CustomerId,
            Score = r.Score,
            Content = r.Content,
            CreatedAt = r.CreatedAt
        });

        var result = await dtoQuery.ToPaginationResponseAsync(request);

        foreach (var item in result.List)
        {
            var order = await _dbContext.Orders.FindAsync(item.OrderId);
            if (order != null)
            {
                item.OrderNo = order.OrderNo;
            }

            var room = await _dbContext.Rooms.FindAsync(item.RoomId);
            if (room != null)
            {
                item.RoomNumber = room.Name;
            }

            var customer = await _dbContext.Customers.FindAsync(item.CustomerId);
            if (customer != null)
            {
                item.CustomerName = customer.Nickname;
                item.CustomerPhone = customer.Phone;
            }
        }

        return result;
    }
}