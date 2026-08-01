using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class AssignCrmVendorOwnerCommand : IRequest<bool>
{
    public CrmVendorAssignOwnerRequest Request { get; set; } = null!;
}

public class AssignCrmVendorOwnerHandler : IRequestHandler<AssignCrmVendorOwnerCommand, bool>
{
    private const string VendorEntityType = CrmCodes.VendorEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 分配厂商负责人处理器。
    /// </summary>
    public AssignCrmVendorOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 编排分配厂商负责人用例。
    /// </summary>
    public async Task<bool> Handle(AssignCrmVendorOwnerCommand request, CancellationToken cancellationToken)
    {
        // 编排分配厂商负责人用例：
        // 规范化厂商编号、获取厂商、确认负责人、写入分配记录。
        var vendorIds = NormalizeVendorIds(request.Request.VendorIds);

        var vendors = await GetVendors(vendorIds, cancellationToken);

        await EnsureTargetOwnerExists(request.Request.OwnerUserId, cancellationToken);

        AssignOwners(vendors, request.Request.OwnerUserId, request.Request.Remark);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 规范化待分配的厂商编号。
    /// </summary>
    private static List<Guid> NormalizeVendorIds(IEnumerable<Guid> vendorIds)
    {
        var normalizedIds = vendorIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            throw new BusinessException(400, "请选择要分配的厂商");
        }

        return normalizedIds;
    }

    /// <summary>
    /// 获取待分配的厂商。
    /// </summary>
    private async Task<List<CrmVendor>> GetVendors(List<Guid> vendorIds, CancellationToken cancellationToken)
    {
        var vendors = await _dbContext.CrmVendors
            .Where(vendor => vendorIds.Contains(vendor.Id) && !vendor.IsDeleted)
            .ToListAsync(cancellationToken);

        if (vendors.Count != vendorIds.Count)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        return vendors;
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
    private void AssignOwners(List<CrmVendor> vendors, Guid? ownerUserId, string? remark)
    {
        var operatorUserId = GetOperatorUserId();
        var normalizedRemark = remark?.Trim() ?? string.Empty;

        foreach (var vendor in vendors)
        {
            var fromOwnerUserId = vendor.OwnerUserId;

            vendor.AssignOwner(ownerUserId);
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                VendorEntityType,
                vendor.Id,
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
        return Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : null;
    }
}
