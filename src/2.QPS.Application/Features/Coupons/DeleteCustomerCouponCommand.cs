using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Coupons;

public class DeleteCustomerCouponCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCustomerCouponCommandHandler : IRequestHandler<DeleteCustomerCouponCommand>
{
    private readonly IDbContext _dbContext;

    public DeleteCustomerCouponCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteCustomerCouponCommand request, CancellationToken cancellationToken)
    {
        var customerCoupon = await _dbContext.CustomerCoupons.FindAsync(request.Id);

        if (customerCoupon == null)
        {
            throw new BusinessException(404, "用户优惠券不存在");
        }

        _dbContext.CustomerCoupons.Remove(customerCoupon);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}