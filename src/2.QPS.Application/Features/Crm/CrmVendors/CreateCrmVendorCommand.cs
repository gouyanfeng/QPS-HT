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

    /// <summary>
    /// 创建厂商处理器。
    /// </summary>
    public CreateCrmVendorHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排创建厂商用例。
    /// </summary>
    public async Task<bool> Handle(CreateCrmVendorCommand request, CancellationToken cancellationToken)
    {
        // 编排创建厂商用例：
        // 规范化名称、校验重复、确认负责人、创建并保存厂商。
        var vendorName = NormalizeVendorDisplayName(request.Request.VendorName);

        var normalizedVendorName = CrmVendorRules.NormalizeVendorName(vendorName);

        await EnsureVendorNameNotExists(normalizedVendorName, cancellationToken);

        await EnsureOwnerExists(request.Request.OwnerUserId, cancellationToken);

        var vendor = CreateVendor(request.Request, vendorName, normalizedVendorName);

        _dbContext.CrmVendors.Add(vendor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 规范化厂商展示名称。
    /// </summary>
    private static string NormalizeVendorDisplayName(string vendorName)
    {
        var normalizedName = vendorName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new BusinessException(400, "请输入厂商名称");
        }

        return normalizedName;
    }

    /// <summary>
    /// 确认厂商名称未被占用。
    /// </summary>
    private async Task EnsureVendorNameNotExists(string normalizedVendorName, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.CrmVendors
            .AnyAsync(
                vendor => !vendor.IsDeleted && vendor.NormalizedVendorName == normalizedVendorName,
                cancellationToken);

        if (exists)
        {
            throw new BusinessException(400, "厂商已存在");
        }
    }

    /// <summary>
    /// 请求带负责人时确认负责人存在。
    /// </summary>
    private async Task EnsureOwnerExists(Guid? ownerUserId, CancellationToken cancellationToken)
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
    /// 根据请求创建厂商实体。
    /// </summary>
    private static CrmVendor CreateVendor(
        CrmVendorCreateRequest request,
        string vendorName,
        string normalizedVendorName)
    {
        return CrmVendor.Create(
            vendorName,
            normalizedVendorName,
            CrmVendorRules.NormalizePriority(request.PriorityLevel),
            request.LatestPurchaseTime,
            request.LatestPurchasePlanName.Trim(),
            request.Remark.Trim(),
            request.OwnerUserId);
    }
}
