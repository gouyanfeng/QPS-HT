using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class AssignCrmVendorOwnerCommand : IRequest<List<CrmVendorDto>>
{
    public CrmVendorAssignOwnerRequest Request { get; set; } = null!;
}

public class AssignCrmVendorOwnerHandler : IRequestHandler<AssignCrmVendorOwnerCommand, List<CrmVendorDto>>
{
    private const string VendorEntityType = "CRM_VENDOR";

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AssignCrmVendorOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<CrmVendorDto>> Handle(AssignCrmVendorOwnerCommand request, CancellationToken cancellationToken)
    {
        var vendorIds = request.Request.VendorIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (vendorIds.Count == 0)
        {
            throw new BusinessException(400, "请选择要分配的厂商");
        }

        var vendors = await _dbContext.CrmVendors
            .Where(vendor => vendorIds.Contains(vendor.Id) && !vendor.IsDeleted)
            .ToListAsync(cancellationToken);

        if (vendors.Count != vendorIds.Count)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        string toOwnerUserName = string.Empty;
        if (request.Request.OwnerUserId.HasValue)
        {
            var owner = await _dbContext.SystemUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == request.Request.OwnerUserId.Value && user.IsActive, cancellationToken);
            if (owner is null)
            {
                throw new BusinessException(404, "负责人不存在");
            }

            toOwnerUserName = owner.RealName;
        }

        var fromOwnerIds = vendors
            .Where(vendor => vendor.OwnerUserId.HasValue)
            .Select(vendor => vendor.OwnerUserId!.Value)
            .Distinct()
            .ToList();
        var fromOwnerLookup = await _dbContext.SystemUsers
            .AsNoTracking()
            .Where(user => fromOwnerIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.RealName, cancellationToken);

        var operatorUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;
        var operatorUserName = _currentUserService.Username ?? string.Empty;
        if (operatorUserId.HasValue)
        {
            operatorUserName = await _dbContext.SystemUsers
                .AsNoTracking()
                .Where(user => user.Id == operatorUserId.Value)
                .Select(user => user.RealName)
                .FirstOrDefaultAsync(cancellationToken) ?? operatorUserName;
        }

        var remark = request.Request.Remark?.Trim() ?? string.Empty;

        foreach (var vendor in vendors)
        {
            var fromOwnerUserId = vendor.OwnerUserId;
            var fromOwnerUserName = fromOwnerUserId.HasValue &&
                fromOwnerLookup.TryGetValue(fromOwnerUserId.Value, out var ownerUserName)
                ? ownerUserName
                : string.Empty;

            vendor.AssignOwner(request.Request.OwnerUserId);
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                VendorEntityType,
                vendor.Id,
                fromOwnerUserId,
                fromOwnerUserName,
                request.Request.OwnerUserId,
                toOwnerUserName,
                operatorUserId,
                operatorUserName,
                remark));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dtos = vendors.Select(vendor => new CrmVendorDto
        {
            Id = vendor.Id,
            VendorName = vendor.VendorName,
            NormalizedVendorName = vendor.NormalizedVendorName,
            PriorityLevel = vendor.PriorityLevel,
            LatestPurchaseTime = vendor.LatestPurchaseTime,
            LatestPurchasePlanName = vendor.LatestPurchasePlanName,
            Remark = vendor.Remark,
            OwnerUserId = vendor.OwnerUserId,
            CreatedAt = vendor.CreatedAt,
            UpdatedAt = vendor.UpdatedAt
        }).ToList();

        await CrmVendorOwners.FillAsync(_dbContext, dtos, cancellationToken);
        return dtos;
    }
}
