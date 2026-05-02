using MediatR;
using QPS.Application.Contracts.Shops;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Shops;

public class GetShopQuery : IRequest<ShopDto>
{
    public Guid Id { get; set; }
}

public class GetShopHandler : IRequestHandler<GetShopQuery, ShopDto>
{
    private readonly IDbContext _dbContext;

    public GetShopHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShopDto> Handle(GetShopQuery request, CancellationToken cancellationToken)
    {
        var shop = await _dbContext.Shops.FindAsync(request.Id, cancellationToken);

        if (shop == null)
        {
            throw new BusinessException(404, "店铺不存在");
        }

        return new ShopDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Address = shop.Address,
            OpeningTime = shop.OpeningTime,
            ClosingTime = shop.ClosingTime,
            AutoPowerOffDelay = shop.AutoPowerOffDelay
        };
    }
}