using MediatR;
using QPS.Application.Contracts.Customers;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Customers;

public class CreateCustomerCommand : IRequest<CustomerDto>
{
    public CustomerCreateRequest Request { get; set; }
}

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IDbContext _dbContext;

    public CreateCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer(
            request.Request.OpenId,
            request.Request.Phone,
            request.Request.Nickname,
            request.Request.AvatarUrl
        );

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

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