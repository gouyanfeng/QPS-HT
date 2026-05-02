using MediatR;
using QPS.Application.Contracts.Shops;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Shops;

public class CreateShopCommand : IRequest<ShopDto>
{
    public ShopCreateRequest Request { get; set; }
}

public class CreateShopHandler : IRequestHandler<CreateShopCommand, ShopDto>
{
    private readonly IDbContext _dbContext;

    public CreateShopHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShopDto> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = Shop.Create(
            request.Request.Name,
            request.Request.Address,
            request.Request.OpeningTime,
            request.Request.ClosingTime,
            request.Request.AutoPowerOffDelay
        );

        _dbContext.Shops.Add(shop);
        await _dbContext.SaveChangesAsync(cancellationToken);

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