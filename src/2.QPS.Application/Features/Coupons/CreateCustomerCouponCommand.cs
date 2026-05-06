using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Coupons;

public class CreateCustomerCouponCommand : IRequest<CustomerCouponDto>
{
    public Guid CouponId { get; set; }
    public Guid CustomerId { get; set; }
}

public class CreateCustomerCouponCommandHandler : IRequestHandler<CreateCustomerCouponCommand, CustomerCouponDto>
{
    private readonly IDbContext _dbContext;

    public CreateCustomerCouponCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerCouponDto> Handle(CreateCustomerCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _dbContext.Coupons.FindAsync(request.CouponId);
        if (coupon == null)
        {
            throw new BusinessException(404, "优惠券不存在");
        }

        var customer = await _dbContext.Customers.FindAsync(request.CustomerId);
        if (customer == null)
        {
            throw new BusinessException(404, "用户不存在");
        }

        var customerCoupon = new CustomerCoupon(request.CouponId, request.CustomerId, "Unused");

        _dbContext.CustomerCoupons.Add(customerCoupon);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CustomerCouponDto
        {
            Id = customerCoupon.Id,
            CouponId = customerCoupon.CouponId,
            CouponTitle = coupon.Title,
            CouponValue = coupon.Value,
            CouponMinConsume = coupon.MinConsume,
            CouponValidTo = coupon.ValidTo,
            CustomerId = customerCoupon.CustomerId,
            CustomerPhone = customer.Phone,
            Status = customerCoupon.Status,
            CreatedAt = customerCoupon.CreatedAt
        };
    }
}