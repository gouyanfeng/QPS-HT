using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Customers;

public class DeleteCustomerCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FindAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}