using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmCustomers;

/// <summary>
/// 删除客户命令
/// </summary>
public class DeleteCrmCustomerCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// 删除客户处理器
/// </summary>
public class DeleteCrmCustomerHandler : IRequestHandler<DeleteCrmCustomerCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteCrmCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteCrmCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmCustomers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        customer.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
