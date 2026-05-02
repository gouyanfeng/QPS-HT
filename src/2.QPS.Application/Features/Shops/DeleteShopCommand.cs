using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Shops;

public class DeleteShopCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteShopHandler : IRequestHandler<DeleteShopCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteShopHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await _dbContext.Shops.FindAsync(request.Id, cancellationToken);

        if (shop == null)
        {
            throw new BusinessException(404, "店铺不存在");
        }

        _dbContext.Shops.Remove(shop);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}