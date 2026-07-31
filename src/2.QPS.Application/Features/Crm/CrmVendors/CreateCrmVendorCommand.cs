using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class CreateCrmVendorCommand : IRequest<CrmVendorDto>
{
    public CrmVendorCreateRequest Request { get; set; } = null!;
}

public class CreateCrmVendorHandler : IRequestHandler<CreateCrmVendorCommand, CrmVendorDto>
{
    private readonly IDbContext _dbContext;

    public CreateCrmVendorHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmVendorDto> Handle(CreateCrmVendorCommand request, CancellationToken cancellationToken)
    {
        var vendorName = request.Request.VendorName.Trim();
        if (string.IsNullOrWhiteSpace(vendorName))
        {
            throw new BusinessException(400, "请输入厂商名称");
        }

        var normalizedVendorName = NormalizeVendorName(vendorName);
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
            NormalizePriority(request.Request.PriorityLevel),
            request.Request.LatestPurchaseTime,
            request.Request.LatestPurchasePlanName.Trim(),
            request.Request.Remark.Trim(),
            request.Request.OwnerUserId);

        _dbContext.CrmVendors.Add(vendor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CrmVendorDto
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
        };

        await CrmVendorOwners.FillAsync(_dbContext, new List<CrmVendorDto> { dto }, cancellationToken);
        return dto;
    }

    private static string NormalizeVendorName(string vendorName)
    {
        return string.Concat(vendorName.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    }

    private static string NormalizePriority(string? priorityLevel)
    {
        return priorityLevel is "High" or "Medium" or "Low" ? priorityLevel : "Medium";
    }
}
