using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Interfaces;
using QPS.Domain.Aggregates.MerchantAggregate;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QPS.Application.Features.Merchants;

public class GetMerchantsQuery : IRequest<List<MerchantDto>> { }

public class GetMerchantsHandler : IRequestHandler<GetMerchantsQuery, List<MerchantDto>>
{
    private readonly IDbContext _dbContext;

    public GetMerchantsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MerchantDto>> Handle(GetMerchantsQuery request, CancellationToken cancellationToken)
    {
        var merchants = await _dbContext.Merchants.ToListAsync(cancellationToken);

        return merchants.Select(m => new MerchantDto
        {
            Id = m.Id,
            Name = m.Name,
            PhoneNumber = m.Phone,
            ExpiryDate = m.ExpiryDate,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}