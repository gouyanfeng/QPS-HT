using MediatR;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Coupons;

public class GetCouponsQuery : PaginationRequest, IRequest<PaginationResponse<CouponDto>>
{
    public string? Title { get; set; }
    public string? CouponType { get; set; }
}

public class GetCouponsHandler : IRequestHandler<GetCouponsQuery, PaginationResponse<CouponDto>>
{
    private readonly IDbContext _dbContext;

    public GetCouponsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Coupons.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Title))
        {
            query = query.Where(c => c.Title.Contains(request.Title));
        }

        if (!string.IsNullOrEmpty(request.CouponType))
        {
            query = query.Where(c => c.CouponType.Contains(request.CouponType));
        }

        var dtoQuery = query.Select(c => new CouponDto
        {
            Id = c.Id,
            Title = c.Title,
            CouponType = c.CouponType,
            Value = c.Value,
            MinConsume = c.MinConsume,
            ValidTo = c.ValidTo
        });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}