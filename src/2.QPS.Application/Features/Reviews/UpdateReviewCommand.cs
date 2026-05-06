using MediatR;
using QPS.Application.Contracts.Reviews;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Reviews;

public class UpdateReviewCommand : IRequest<ReviewDto>
{
    public Guid Id { get; set; }
    public ReviewUpdateRequest Request { get; set; }
}

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, ReviewDto>
{
    private readonly IDbContext _dbContext;

    public UpdateReviewCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReviewDto> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Reviews.FindAsync(request.Id);

        if (review == null)
        {
            throw new BusinessException(404, "评价不存在");
        }

        if (request.Request.Score < 1 || request.Request.Score > 5)
        {
            throw new BusinessException(400, "评分必须在1-5之间");
        }

        var oldScore = review.Score;
        review.Update(request.Request.Score, request.Request.Content);

        var room = await _dbContext.Rooms.FindAsync(review.RoomId);
        if (room != null)
        {
            room.UpdateRating(oldScore, request.Request.Score);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var order = await _dbContext.Orders.FindAsync(review.OrderId);
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