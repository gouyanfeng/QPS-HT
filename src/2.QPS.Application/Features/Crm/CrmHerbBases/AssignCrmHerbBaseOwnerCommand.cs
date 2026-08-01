using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

public class AssignCrmHerbBaseOwnerCommand : IRequest<bool>
{
    public CrmHerbBaseAssignOwnerRequest Request { get; set; } = null!;
}

public class AssignCrmHerbBaseOwnerHandler : IRequestHandler<AssignCrmHerbBaseOwnerCommand, bool>
{
    private const string HerbBaseEntityType = CrmCodes.HerbBaseEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 分配药材基地负责人处理器。
    /// </summary>
    public AssignCrmHerbBaseOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 编排分配药材基地负责人用例。
    /// </summary>
    public async Task<bool> Handle(AssignCrmHerbBaseOwnerCommand request, CancellationToken cancellationToken)
    {
        // 编排分配药材基地负责人用例：
        // 规范化客户编号、获取客户、确认负责人、写入分配记录。
        var herbBaseIds = NormalizeHerbBaseIds(request.Request.HerbBaseIds);

        var customers = await GetCustomers(herbBaseIds, cancellationToken);

        await EnsureTargetOwnerExists(request.Request.OwnerUserId, cancellationToken);

        AssignOwners(customers, request.Request.OwnerUserId, request.Request.Remark);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 规范化待分配的药材基地编号。
    /// </summary>
    private static List<Guid> NormalizeHerbBaseIds(IEnumerable<Guid> herbBaseIds)
    {
        var normalizedIds = herbBaseIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            throw new BusinessException(400, "请选择要分配的药材基地");
        }

        return normalizedIds;
    }

    /// <summary>
    /// 获取待分配的药材基地客户。
    /// </summary>
    private async Task<List<CrmHerbBase>> GetCustomers(List<Guid> herbBaseIds, CancellationToken cancellationToken)
    {
        var customers = await _dbContext.CrmHerbBases
            .Where(customer => herbBaseIds.Contains(customer.Id) && !customer.IsDeleted)
            .ToListAsync(cancellationToken);

        if (customers.Count != herbBaseIds.Count)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        return customers;
    }

    /// <summary>
    /// 请求带负责人时确认负责人存在。
    /// </summary>
    private async Task EnsureTargetOwnerExists(Guid? ownerUserId, CancellationToken cancellationToken)
    {
        if (!ownerUserId.HasValue)
        {
            return;
        }

        var ownerExists = await _dbContext.SystemUsers
            .AsNoTracking()
            .AnyAsync(user => user.Id == ownerUserId.Value && user.IsActive, cancellationToken);

        if (!ownerExists)
        {
            throw new BusinessException(404, "负责人不存在");
        }
    }

    /// <summary>
    /// 批量分配负责人并记录流转。
    /// </summary>
    private void AssignOwners(List<CrmHerbBase> customers, Guid? ownerUserId, string? remark)
    {
        var operatorUserId = GetOperatorUserId();
        var normalizedRemark = remark?.Trim() ?? string.Empty;

        foreach (var customer in customers)
        {
            var fromOwnerUserId = customer.OwnerUserId;

            customer.AssignOwner(ownerUserId);
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                HerbBaseEntityType,
                customer.Id,
                fromOwnerUserId,
                ownerUserId,
                operatorUserId,
                normalizedRemark));
        }
    }

    /// <summary>
    /// 获取当前操作人编号。
    /// </summary>
    private Guid? GetOperatorUserId()
    {
        return Guid.TryParse(_currentUserService.UserId, out var operatorUserId)
            ? operatorUserId
            : null;
    }
}
