using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class DeleteCrmVendorPurchasePlanCommand : IRequest<bool>
{
    public Guid VendorId { get; set; }

    public Guid Id { get; set; }
}

public class DeleteCrmVendorPurchasePlanHandler : IRequestHandler<DeleteCrmVendorPurchasePlanCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteCrmVendorPurchasePlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteCrmVendorPurchasePlanCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.CrmVendors
            .FirstOrDefaultAsync(item => item.Id == request.VendorId && !item.IsDeleted, cancellationToken);
        if (vendor == null)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        var plan = await _dbContext.CrmVendorPurchasePlans
            .FirstOrDefaultAsync(item => item.Id == request.Id && item.VendorId == request.VendorId && !item.IsDeleted, cancellationToken);
        if (plan == null)
        {
            throw new BusinessException(404, "采购计划不存在");
        }

        plan.IsDeleted = true;
        await CrmVendorPurchasePlans.RefreshLatestAsync(_dbContext, vendor, cancellationToken, excludedPlanId: plan.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
