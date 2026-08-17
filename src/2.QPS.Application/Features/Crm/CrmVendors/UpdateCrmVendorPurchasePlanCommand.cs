using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class UpdateCrmVendorPurchasePlanCommand : IRequest<bool>
{
    public Guid VendorId { get; set; }

    public Guid Id { get; set; }

    public CrmVendorPurchasePlanCreateRequest Request { get; set; } = null!;
}

public class UpdateCrmVendorPurchasePlanHandler : IRequestHandler<UpdateCrmVendorPurchasePlanCommand, bool>
{
    private readonly IDbContext _dbContext;

    public UpdateCrmVendorPurchasePlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateCrmVendorPurchasePlanCommand request, CancellationToken cancellationToken)
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

        var purchasePlanName = request.Request.PurchasePlanName.Trim();
        if (string.IsNullOrWhiteSpace(purchasePlanName))
        {
            throw new BusinessException(400, "请输入采购计划名称");
        }

        plan.Update(
            purchasePlanName,
            request.Request.PurchaseTime,
            request.Request.Products.Trim(),
            request.Request.PageUrl.Trim(),
            request.Request.Remark.Trim());

        await CrmVendorPurchasePlans.RefreshLatestAsync(_dbContext, vendor, cancellationToken, plan);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
