using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Coupons;

public class GetCouponQuery : IRequest<CouponDto>
{
    public Guid Id { get; set; }
}

public class GetCouponHandler : IRequestHandler<GetCouponQuery, CouponDto>
{
    private readonly IDbContext _dbContext;

    public GetCouponHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CouponDto> Handle(GetCouponQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _dbContext.Coupons.FindAsync(request.Id, cancellationToken);

        if (coupon == null)
        {
            throw new BusinessException(404, "优惠券不存在");
        }

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