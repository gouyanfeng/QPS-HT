using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class CreateCrmVendorPurchasePlanCommand : IRequest<bool>
{
    public Guid VendorId { get; set; }

    public CrmVendorPurchasePlanCreateRequest Request { get; set; } = null!;
}

public class CreateCrmVendorPurchasePlanHandler : IRequestHandler<CreateCrmVendorPurchasePlanCommand, bool>
{
    private readonly IDbContext _dbContext;

    public CreateCrmVendorPurchasePlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(CreateCrmVendorPurchasePlanCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.CrmVendors
            .FirstOrDefaultAsync(vendor => vendor.Id == request.VendorId && !vendor.IsDeleted, cancellationToken);
        if (vendor == null)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        var purchasePlanName = request.Request.PurchasePlanName.Trim();
        if (string.IsNullOrWhiteSpace(purchasePlanName))
        {
            throw new BusinessException(400, "请输入采购计划名称");
        }

        var purchasePlan = CrmVendorPurchasePlan.Create(
            vendor.Id,
            purchasePlanName,
            request.Request.PurchaseTime,
            request.Request.Products.Trim(),
            request.Request.PageUrl.Trim(),
            request.Request.Remark.Trim());

        _dbContext.CrmVendorPurchasePlans.Add(purchasePlan);
        await CrmVendorPurchasePlans.RefreshLatestAsync(_dbContext, vendor, cancellationToken, purchasePlan);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
