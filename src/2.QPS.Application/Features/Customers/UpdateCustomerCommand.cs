using MediatR;
using QPS.Application.Contracts.Customers;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Customers;

public class UpdateCustomerCommand : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
    public CustomerUpdateRequest Request { get; set; }
}

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IDbContext _dbContext;

    public UpdateCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FindAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        customer.Update(
            request.Request.Phone,
            request.Request.Nickname,
            request.Request.AvatarUrl
        );

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