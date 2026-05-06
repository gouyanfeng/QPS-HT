using MediatR;
using QPS.Application.Contracts.Reviews;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Reviews;

public class CreateReviewCommand : IRequest<ReviewDto>
{
    public ReviewCreateRequest Request { get; set; }
}

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IDbContext _dbContext;

    public CreateReviewCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync(request.Request.OrderId);
        if (order == null)
        {
            throw new BusinessException(404, "订单不存在");
        }

        if (order.Status != OrderStatus.Completed)
        {
            throw new BusinessException(400, "只有已完成的订单才能评价");
        }

        var existingReview = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.OrderId == request.Request.OrderId);
        if (existingReview != null)
        {
            throw new BusinessException(400, "该订单已评价");
        }

        var room = await _dbContext.Rooms.FindAsync(order.RoomId);
        if (room == null)
        {
            throw new BusinessException(404, "房间不存在");
        }

        var customer = await _dbContext.Customers.FindAsync(order.CustomerId);
        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        if (request.Request.Score < 1 || request.Request.Score > 5)
        {
            throw new BusinessException(400, "评分必须在1-5之间");
        }

        var review = new Review(
            order.Id,
            order.RoomId,
            customer.Id,
            request.Request.Score,
            request.Request.Content
        );

        _dbContext.Reviews.Add(review);

        room.AddRating(request.Request.Score);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            OrderId = review.OrderId,
            OrderNo = order.OrderNo,
            RoomId = review.RoomId,
            RoomNumber = room.Name,
            CustomerId = review.CustomerId,
            CustomerName = customer.Nickname,
            CustomerPhone = customer.Phone,
            Score = review.Score,
            Content = review.Content,
            CreatedAt = review.CreatedAt
        };
    }
}