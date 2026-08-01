using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class CreateCrmVendorCommand : IRequest<bool>
{
    public CrmVendorCreateRequest Request { get; set; } = null!;
}

public class CreateCrmVendorHandler : IRequestHandler<CreateCrmVendorCommand, bool>
{
    private readonly IDbContext _dbContext;

    public CreateCrmVendorHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(CreateCrmVendorCommand request, CancellationToken cancellationToken)
    {
        var vendorName = request.Request.VendorName.Trim();
        if (string.IsNullOrWhiteSpace(vendorName))
        {
            throw new BusinessException(400, "请输入厂商名称");
        }

        var normalizedVendorName = CrmVendorRules.NormalizeVendorName(vendorName);
        var exists = await _dbContext.CrmVendors
            .AnyAsync(vendor => !vendor.IsDeleted && vendor.NormalizedVendorName == normalizedVendorName, cancellationToken);
        if (exists)
        {
            throw new BusinessException(400, "厂商已存在");
        }

        if (request.Request.OwnerUserId.HasValue)
        {
            var ownerExists = await _dbContext.SystemUsers
                .AsNoTracking()
                .AnyAsync(user => user.Id == request.Request.OwnerUserId.Value && user.IsActive, cancellationToken);
            if (!ownerExists)
            {
                throw new BusinessException(404, "负责人不存在");
            }
        }

        var vendor = CrmVendor.Create(
            vendorName,
            normalizedVendorName,
            CrmVendorRules.NormalizePriority(request.Request.PriorityLevel),
            request.Request.LatestPurchaseTime,
            request.Request.LatestPurchasePlanName.Trim(),
            request.Request.Remark.Trim(),
            request.Request.OwnerUserId);

        _dbContext.CrmVendors.Add(vendor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

}
