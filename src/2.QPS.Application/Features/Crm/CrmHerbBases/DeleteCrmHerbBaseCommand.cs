using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
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

    /// <summary>
    /// 删除药材基地处理器。
    /// </summary>
    public DeleteCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排删除药材基地用例。
    /// </summary>
    public async Task<bool> Handle(DeleteCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        // 编排删除药材基地用例：
        // 获取客户、标记删除、保存。
        var customer = await GetCustomer(request.Id, cancellationToken);

        customer.IsDeleted = true;
        await SyncSubjectScaleAsync(customer, cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (customer.HerbBaseSubjectId.HasValue)
        {
            var scoreInput = await CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(_dbContext, customer.HerbBaseSubjectId.Value, cancellationToken);
            if (scoreInput != null)
            {
                var subject = await _dbContext.CrmHerbBaseSubjects.FirstOrDefaultAsync(item => item.Id == customer.HerbBaseSubjectId.Value, cancellationToken);
                if (subject != null)
                {
                    subject.RecalculateScoreGrade(scoreInput);
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// 获取要删除的药材基地客户。
    /// </summary>
    private async Task<CrmHerbBase> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        return customer;
    }

    private async Task SyncSubjectScaleAsync(CrmHerbBase herbBase, CancellationToken cancellationToken)
    {
        if (!herbBase.HerbBaseSubjectId.HasValue)
        {
            return;
        }

        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(item => item.Id == herbBase.HerbBaseSubjectId.Value, cancellationToken);
        if (subject == null)
        {
            return;
        }

        var remainingScale = await _dbContext.CrmHerbBases
            .Where(item =>
                item.HerbBaseSubjectId == herbBase.HerbBaseSubjectId.Value &&
                item.Id != herbBase.Id)
            .SumAsync(item => item.Scale ?? 0, cancellationToken);

        subject.UpdateScale(remainingScale);
    }
}



