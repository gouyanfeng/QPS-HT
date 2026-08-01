using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 删除药材基地命令
/// </summary>
public class DeleteCrmHerbBaseCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// 删除药材基地处理器
/// </summary>
public class DeleteCrmHerbBaseHandler : IRequestHandler<DeleteCrmHerbBaseCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        customer.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}



