using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Coupons;

public class UpdateCouponCommand : IRequest<CouponDto>
{
    public Guid Id { get; set; }
    public CouponUpdateRequest Request { get; set; }
}

public class UpdateCouponHandler : IRequestHandler<UpdateCouponCommand, CouponDto>
{
    private readonly IDbContext _dbContext;

    public UpdateCouponHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _dbContext.Coupons.FindAsync(request.Id, cancellationToken);

        if (coupon == null)
        {
            throw new BusinessException(404, "优惠券不存在");
        }

        coupon.Update(
            request.Request.Title,
            request.Request.CouponType,
            request.Request.Value,
            request.Request.MinConsume,
            request.Request.ValidTo
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CouponDto
        {
            Id = coupon.Id,
            Title = coupon.Title,
            CouponType = coupon.CouponType,
            Value = coupon.Value,
            MinConsume = coupon.MinConsume,
            ValidTo = coupon.ValidTo
        };
    }
}