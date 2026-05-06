using MediatR;
using QPS.Application.Contracts.Reviews;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Reviews;

public class GetReviewQuery : IRequest<ReviewDto>
{
    public Guid Id { get; set; }
}

public class GetReviewQueryHandler : IRequestHandler<GetReviewQuery, ReviewDto>
{
    private readonly IDbContext _dbContext;

    public GetReviewQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReviewDto> Handle(GetReviewQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Reviews.FindAsync(request.Id);

        if (review == null)
        {
            throw new BusinessException(404, "评价不存在");
        }

        var order = await _dbContext.Orders.FindAsync(review.OrderId);
        var room = await _dbContext.Rooms.FindAsync(review.RoomId);
        var customer = await _dbContext.Customers.FindAsync(review.CustomerId);

        return new ReviewDto
        {
            Id = review.Id,
            OrderId = review.OrderId,
            OrderNo = order?.OrderNo,
            RoomId = review.RoomId,
            RoomNumber = room?.Name,
            CustomerId = review.CustomerId,
            CustomerName = customer?.Nickname,
            CustomerPhone = customer?.Phone,
            Score = review.Score,
            Content = review.Content,
            CreatedAt = review.CreatedAt
        };
    }
}