using MediatR;
using QPS.Application.Contracts.Shops;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Shops;

public class GetShopsQuery : PaginationRequest, IRequest<PaginationResponse<ShopDto>>
{
    public string? Name { get; set; }
}

public class GetShopsHandler : IRequestHandler<GetShopsQuery, PaginationResponse<ShopDto>>
{
    private readonly IDbContext _dbContext;

    public GetShopsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<ShopDto>> Handle(GetShopsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Shops.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(s => s.Name.Contains(request.Name));
        }

        var dtoQuery = query.Select(s => new ShopDto
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            OpeningTime = s.OpeningTime,
            ClosingTime = s.ClosingTime,
            AutoPowerOffDelay = s.AutoPowerOffDelay
        });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}