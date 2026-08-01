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

    public AssignCrmVendorOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(AssignCrmVendorOwnerCommand request, CancellationToken cancellationToken)
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

        var operatorUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        var remark = request.Request.Remark?.Trim() ?? string.Empty;

        foreach (var vendor in vendors)
        {
            var fromOwnerUserId = vendor.OwnerUserId;

            vendor.AssignOwner(request.Request.OwnerUserId);
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                VendorEntityType,
                vendor.Id,
                fromOwnerUserId,
                request.Request.OwnerUserId,
                operatorUserId,
                remark));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
