using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Coupons;

public class CreateCouponCommand : IRequest<CouponDto>
{
    public CouponCreateRequest Request { get; set; }
}

public class CreateCouponHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly IDbContext _dbContext;

    public CreateCouponHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = new Coupon(
            request.Request.Title,
            request.Request.CouponType,
            request.Request.Value,
            request.Request.MinConsume,
            request.Request.ValidTo
        );

        _dbContext.Coupons.Add(coupon);
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