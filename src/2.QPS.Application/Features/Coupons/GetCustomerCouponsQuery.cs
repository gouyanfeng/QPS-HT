using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Coupons;

public class GetCustomerCouponsQuery : PaginationRequest, IRequest<PaginationResponse<CustomerCouponDto>>
{
    public Guid? CustomerId { get; set; }
    public string Status { get; set; }
}

public class GetCustomerCouponsQueryHandler : IRequestHandler<GetCustomerCouponsQuery, PaginationResponse<CustomerCouponDto>>
{
    private readonly IDbContext _dbContext;

    public GetCustomerCouponsQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CustomerCouponDto>> Handle(GetCustomerCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.CustomerCoupons.AsNoTracking();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(cc => cc.CustomerId == request.CustomerId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(cc => cc.Status == request.Status);
        }

        var dtoQuery = query.Select(cc => new CustomerCouponDto
        {
            Id = cc.Id,
            CouponId = cc.CouponId,
            CustomerId = cc.CustomerId,
            Status = cc.Status,
            CreatedAt = cc.CreatedAt
        });

        var result = await dtoQuery.ToPaginationResponseAsync(request);

        foreach (var item in result.List)
        {
            var coupon = await _dbContext.Coupons.FindAsync(item.CouponId);
            if (coupon != null)
            {
                item.CouponTitle = coupon.Title;
                item.CouponValue = coupon.Value;
                item.CouponMinConsume = coupon.MinConsume;
                item.CouponValidTo = coupon.ValidTo;
            }

            var customer = await _dbContext.Customers.FindAsync(item.CustomerId);
            if (customer != null)
            {
                item.CustomerPhone = customer.Phone;
            }
        }

        return result;
    }
}