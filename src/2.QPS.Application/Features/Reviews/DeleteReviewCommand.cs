using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Reviews;

public class DeleteReviewCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IDbContext _dbContext;

    public DeleteReviewCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Reviews.FindAsync(request.Id);

        if (review == null)
        {
            throw new BusinessException(404, "评价不存在");
        }

        var room = await _dbContext.Rooms.FindAsync(review.RoomId);
        if (room != null)
        {
            room.RemoveRating(review.Score);
        }

        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}