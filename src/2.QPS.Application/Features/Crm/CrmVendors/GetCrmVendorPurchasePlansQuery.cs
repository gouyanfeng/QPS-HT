using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class GetCrmVendorPurchasePlansQuery : PaginationRequest, IRequest<PaginationResponse<CrmVendorPurchasePlanDto>>
{
    public Guid VendorId { get; set; }
}

public class GetCrmVendorPurchasePlansHandler : IRequestHandler<GetCrmVendorPurchasePlansQuery, PaginationResponse<CrmVendorPurchasePlanDto>>
{
    private readonly IDbContext _dbContext;

    public GetCrmVendorPurchasePlansHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CrmVendorPurchasePlanDto>> Handle(
        GetCrmVendorPurchasePlansQuery request,
        CancellationToken cancellationToken)
    {
        var vendorExists = await _dbContext.CrmVendors
            .AnyAsync(vendor => vendor.Id == request.VendorId && !vendor.IsDeleted, cancellationToken);

        if (!vendorExists)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.CrmVendorPurchasePlans
            .Where(plan => !plan.IsDeleted && plan.VendorId == request.VendorId);

        var totalCount = await query.CountAsync(cancellationToken);

        var isAscending = request.SortDirection.Equals("Ascending", StringComparison.OrdinalIgnoreCase);
        var orderedQuery = request.SortField switch
        {
            nameof(CrmVendorPurchasePlanDto.PurchasePlanName) => isAscending
                ? query.OrderBy(plan => plan.PurchasePlanName).ThenByDescending(plan => plan.CreatedAt)
                : query.OrderByDescending(plan => plan.PurchasePlanName).ThenByDescending(plan => plan.CreatedAt),
            nameof(CrmVendorPurchasePlanDto.CreatedAt) => isAscending
                ? query.OrderBy(plan => plan.CreatedAt)
                : query.OrderByDescending(plan => plan.CreatedAt),
            nameof(CrmVendorPurchasePlanDto.UpdatedAt) => isAscending
                ? query.OrderBy(plan => plan.UpdatedAt)
                : query.OrderByDescending(plan => plan.UpdatedAt),
            _ => isAscending
                ? query.OrderBy(plan => plan.PurchaseTime).ThenBy(plan => plan.CreatedAt)
                : query.OrderByDescending(plan => plan.PurchaseTime).ThenByDescending(plan => plan.CreatedAt)
        };

        var plans = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(plan => new CrmVendorPurchasePlanDto
            {
                Id = plan.Id,
                VendorId = plan.VendorId,
                PurchasePlanName = plan.PurchasePlanName,
                PurchaseTime = plan.PurchaseTime,
                Products = plan.Products,
                PageUrl = plan.PageUrl,
                Remark = plan.Remark,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginationResponse<CrmVendorPurchasePlanDto>(plans, totalCount, page, pageSize);
    }
}


