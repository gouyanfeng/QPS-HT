using MediatR;
using QPS.Application.Contracts.Customers;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Customers;

public class GetCustomersQuery : PaginationRequest, IRequest<PaginationResponse<CustomerDto>>
{
    public string? Phone { get; set; }
    public string? Nickname { get; set; }
}

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, PaginationResponse<CustomerDto>>
{
    private readonly IDbContext _dbContext;

    public GetCustomersHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Phone))
        {
            query = query.Where(c => c.Phone.Contains(request.Phone));
        }

        if (!string.IsNullOrEmpty(request.Nickname))
        {
            query = query.Where(c => c.Nickname.Contains(request.Nickname));
        }

        var dtoQuery = query.Select(c => new CustomerDto
        {
            Id = c.Id,
            OpenId = c.OpenId,
            Phone = c.Phone,
            Nickname = c.Nickname,
            AvatarUrl = c.AvatarUrl
        });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}