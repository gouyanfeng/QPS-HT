using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Coupons;

public class GetCustomerCouponQuery : IRequest<CustomerCouponDto>
{
    public Guid Id { get; set; }
}

public class GetCustomerCouponQueryHandler : IRequestHandler<GetCustomerCouponQuery, CustomerCouponDto>
{
    private readonly IDbContext _dbContext;

    public GetCustomerCouponQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerCouponDto> Handle(GetCustomerCouponQuery request, CancellationToken cancellationToken)
    {
        var customerCoupon = await _dbContext.CustomerCoupons.FindAsync(request.Id);

        if (customerCoupon == null)
        {
            throw new BusinessException(404, "用户优惠券不存在");
        }

        var coupon = await _dbContext.Coupons.FindAsync(customerCoupon.CouponId);
        var customer = await _dbContext.Customers.FindAsync(customerCoupon.CustomerId);

        return new CustomerCouponDto
        {
            Id = customerCoupon.Id,
            CouponId = customerCoupon.CouponId,
            CouponTitle = coupon?.Title,
            CouponValue = coupon?.Value ?? 0,
            CouponMinConsume = coupon?.MinConsume ?? 0,
            CouponValidTo = coupon?.ValidTo ?? DateTime.MinValue,
            CustomerId = customerCoupon.CustomerId,
            CustomerPhone = customer?.Phone,
            Status = customerCoupon.Status,
            CreatedAt = customerCoupon.CreatedAt
        };
    }
}