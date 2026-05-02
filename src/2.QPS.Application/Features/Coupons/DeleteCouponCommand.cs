using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Coupons;

public class DeleteCouponCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteCouponHandler : IRequestHandler<DeleteCouponCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteCouponHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _dbContext.Coupons.FindAsync(request.Id, cancellationToken);

        if (coupon == null)
        {
            throw new BusinessException(404, "优惠券不存在");
        }

        _dbContext.Coupons.Remove(coupon);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}