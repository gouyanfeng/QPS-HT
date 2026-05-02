using MediatR;
using QPS.Application.Contracts.Shops;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Shops;

public class UpdateShopCommand : IRequest<ShopDto>
{
    public Guid Id { get; set; }
    public ShopUpdateRequest Request { get; set; }
}

public class UpdateShopHandler : IRequestHandler<UpdateShopCommand, ShopDto>
{
    private readonly IDbContext _dbContext;

    public UpdateShopHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShopDto> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await _dbContext.Shops.FindAsync(request.Id, cancellationToken);

        if (shop == null)
        {
            throw new BusinessException(404, "店铺不存在");
        }

        shop.Update(
            request.Request.Name,
            request.Request.Address,
            request.Request.OpeningTime,
            request.Request.ClosingTime,
            request.Request.AutoPowerOffDelay
        );

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