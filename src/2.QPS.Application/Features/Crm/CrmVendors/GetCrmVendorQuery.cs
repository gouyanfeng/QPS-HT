using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class GetCrmVendorQuery : IRequest<CrmVendorDto>
{
    public Guid Id { get; set; }
}

public class GetCrmVendorHandler : IRequestHandler<GetCrmVendorQuery, CrmVendorDto>
{
    private const string VendorEntityType = CrmCodes.VendorEntityType;
    private const string PurchaseProductAttributeCode = "PURCHASE_PRODUCT";

    private readonly IDbContext _dbContext;

    public GetCrmVendorHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmVendorDto> Handle(GetCrmVendorQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.CrmVendors
            .FirstOrDefaultAsync(item => item.Id == request.Id && !item.IsDeleted, cancellationToken);

        if (vendor is null)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        var contacts = await _dbContext.CrmContacts
            .Where(contact =>
                !contact.IsDeleted &&
                contact.EntityType == VendorEntityType &&
                contact.EntityId == vendor.Id)
            .OrderByDescending(contact => contact.IsPrimary)
            .ThenBy(contact => contact.CreatedAt)
            .Select(contact => new CrmContactDto
            {
                Id = contact.Id,
                EntityType = contact.EntityType,
                EntityId = contact.EntityId,
                ContactName = contact.ContactName,
                Phone = contact.Phone,
                PhoneType = contact.PhoneType,
                Wechat = contact.Wechat,
                RoleName = contact.RoleName,
                IsPrimary = contact.IsPrimary,
                Status = contact.Status,
                Remark = contact.Remark,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var products = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == VendorEntityType &&
                attribute.EntityId == vendor.Id &&
                attribute.AttributeCode == PurchaseProductAttributeCode)
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.AttributeValue)
            .Take(80)
            .Select(attribute => new CrmVendorProductDto
            {
                Id = attribute.Id,
                ProductName = attribute.AttributeValue,
                SortOrder = attribute.SortOrder,
                Remark = attribute.Remark
            })
            .ToListAsync(cancellationToken);

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
            PrimaryContactName = contacts.FirstOrDefault()?.ContactName ?? string.Empty,
            PrimaryContactPhone = contacts.FirstOrDefault()?.Phone ?? string.Empty,
            PurchasePlanCount = await _dbContext.CrmVendorPurchasePlans.CountAsync(plan => !plan.IsDeleted && plan.VendorId == vendor.Id, cancellationToken),
            ProductCount = await _dbContext.CrmBusinessEntityAttributes.CountAsync(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == VendorEntityType &&
                attribute.EntityId == vendor.Id &&
                attribute.AttributeCode == PurchaseProductAttributeCode,
                cancellationToken),
            ContactCount = contacts.Count,
            CreatedAt = vendor.CreatedAt,
            UpdatedAt = vendor.UpdatedAt,
            Contacts = contacts,
            Products = products
        };

        await CrmVendorOwners.FillAsync(_dbContext, new List<CrmVendorDto> { dto }, cancellationToken);
        return dto;
    }
}


