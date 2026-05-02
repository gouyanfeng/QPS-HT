using MediatR;
using QPS.Application.Contracts.Customers;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Customers;

public class GetCustomerQuery : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
}

public class GetCustomerHandler : IRequestHandler<GetCustomerQuery, CustomerDto>
{
    private readonly IDbContext _dbContext;

    public GetCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FindAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        return new CustomerDto
        {
            Id = customer.Id,
            OpenId = customer.OpenId,
            Phone = customer.Phone,
            Nickname = customer.Nickname,
            AvatarUrl = customer.AvatarUrl
        };
    }
}