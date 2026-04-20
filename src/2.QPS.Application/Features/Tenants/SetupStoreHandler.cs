using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.MerchantAggregate;

namespace QPS.Application.Features.Tenants;

public class SetupStoreCommand : IRequest<Guid>
{
    public string StoreName { get; set; }
    public string PhoneNumber { get; set; }
    public int PowerOffDelayMinutes { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
}

public class SetupStoreHandler : IRequestHandler<SetupStoreCommand, Guid>
{
    private readonly IDbContext _dbContext;

    public SetupStoreHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(SetupStoreCommand request, CancellationToken cancellationToken)
    {
        var storeSettings = new StoreSettings(
            request.PowerOffDelayMinutes,
            request.OpeningTime,
            request.ClosingTime
        );

        var merchant = new Merchant(request.StoreName, request.PhoneNumber, storeSettings);
        _dbContext.Merchants.Add(merchant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return merchant.Id;
    }
}