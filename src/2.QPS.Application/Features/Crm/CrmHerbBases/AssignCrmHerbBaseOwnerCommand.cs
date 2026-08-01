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

    public AssignCrmHerbBaseOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(AssignCrmHerbBaseOwnerCommand request, CancellationToken cancellationToken)
    {
        var ownerUserId = request.Request.OwnerUserId;
        var herbBaseIds = request.Request.HerbBaseIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (herbBaseIds.Count == 0)
        {
            throw new BusinessException(400, "请选择要分配的药材基地");
        }

        var customers = await _dbContext.CrmHerbBases
            .Where(customer => herbBaseIds.Contains(customer.Id) && !customer.IsDeleted)
            .ToListAsync(cancellationToken);

        if (customers.Count != herbBaseIds.Count)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        await EnsureTargetOwnerExists(ownerUserId, cancellationToken);
        var operatorUserId = GetOperatorUserId();
        var remark = request.Request.Remark?.Trim() ?? string.Empty;

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
                remark));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

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

    private Guid? GetOperatorUserId()
    {
        return Guid.TryParse(_currentUserService.UserId, out var operatorUserId)
            ? operatorUserId
            : null;
    }
}




