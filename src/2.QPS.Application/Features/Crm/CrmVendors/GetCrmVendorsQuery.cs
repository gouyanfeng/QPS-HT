using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Extensions;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmVendors;

public class GetCrmVendorsQuery : PaginationRequest, IRequest<PaginationResponse<CrmVendorDto>>
{
    public string? Keyword { get; set; }

    public string? PriorityLevel { get; set; }

    public bool? HasPhone { get; set; }

    public bool? HasProduct { get; set; }
}

public class GetCrmVendorsHandler : IRequestHandler<GetCrmVendorsQuery, PaginationResponse<CrmVendorDto>>
{
    private const string VendorEntityType = CrmCodes.VendorEntityType;
    private const string PurchaseProductAttributeCode = "PURCHASE_PRODUCT";
    private const string InvalidContactStatus = "INVALID";

    private readonly IDbContext _dbContext;

    public GetCrmVendorsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CrmVendorDto>> Handle(GetCrmVendorsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.CrmVendors
            .Where(vendor => !vendor.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword!;
            query = query.Where(vendor =>
                vendor.VendorName.Contains(keyword) ||
                vendor.NormalizedVendorName.Contains(keyword) ||
                vendor.LatestPurchasePlanName.Contains(keyword) ||
                _dbContext.CrmContacts.Any(contact =>
                    !contact.IsDeleted &&
                    contact.EntityType == VendorEntityType &&
                    contact.EntityId == vendor.Id &&
                    (contact.ContactName.Contains(keyword) || contact.Phone.Contains(keyword))) ||
                _dbContext.CrmBusinessEntityAttributes.Any(attribute =>
                    !attribute.IsDeleted &&
                    attribute.EntityType == VendorEntityType &&
                    attribute.EntityId == vendor.Id &&
                    attribute.AttributeCode == PurchaseProductAttributeCode &&
                    attribute.AttributeValue.Contains(keyword)) ||
                _dbContext.CrmVendorPurchasePlans.Any(plan =>
                    !plan.IsDeleted &&
                    plan.VendorId == vendor.Id &&
                    plan.PurchasePlanName.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(request.PriorityLevel))
        {
            query = query.Where(vendor => vendor.PriorityLevel == request.PriorityLevel);
        }

        if (request.HasPhone.HasValue)
        {
            query = request.HasPhone.Value
                ? query.Where(vendor => _dbContext.CrmContacts.Any(contact =>
                    !contact.IsDeleted &&
                    contact.EntityType == VendorEntityType &&
                    contact.EntityId == vendor.Id &&
                    contact.Phone != string.Empty))
                : query.Where(vendor => !_dbContext.CrmContacts.Any(contact =>
                    !contact.IsDeleted &&
                    contact.EntityType == VendorEntityType &&
                    contact.EntityId == vendor.Id &&
                    contact.Phone != string.Empty));
        }

        if (request.HasProduct.HasValue)
        {
            query = request.HasProduct.Value
                ? query.Where(vendor => _dbContext.CrmBusinessEntityAttributes.Any(attribute =>
                    !attribute.IsDeleted &&
                    attribute.EntityType == VendorEntityType &&
                    attribute.EntityId == vendor.Id &&
                    attribute.AttributeCode == PurchaseProductAttributeCode))
                : query.Where(vendor => !_dbContext.CrmBusinessEntityAttributes.Any(attribute =>
                    !attribute.IsDeleted &&
                    attribute.EntityType == VendorEntityType &&
                    attribute.EntityId == vendor.Id &&
                    attribute.AttributeCode == PurchaseProductAttributeCode));
        }

        var dtoQuery = query.Select(vendor => new CrmVendorDto
        {
            Id = vendor.Id,
            VendorName = vendor.VendorName,
            NormalizedVendorName = vendor.NormalizedVendorName,
            PriorityLevel = vendor.PriorityLevel,
            LatestPurchaseTime = vendor.LatestPurchaseTime,
            LatestPurchasePlanName = vendor.LatestPurchasePlanName,
            Remark = vendor.Remark,
            OwnerUserId = vendor.OwnerUserId,
            PrimaryContactName = _dbContext.CrmContacts
                .Where(contact =>
                    !contact.IsDeleted &&
                    contact.EntityType == VendorEntityType &&
                    contact.EntityId == vendor.Id &&
                    contact.Status != InvalidContactStatus)
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenBy(contact => contact.CreatedAt)
                .Select(contact => contact.ContactName)
                .FirstOrDefault() ?? string.Empty,
            PrimaryContactPhone = _dbContext.CrmContacts
                .Where(contact =>
                    !contact.IsDeleted &&
                    contact.EntityType == VendorEntityType &&
                    contact.EntityId == vendor.Id &&
                    contact.Status != InvalidContactStatus)
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenBy(contact => contact.CreatedAt)
                .Select(contact => contact.Phone)
                .FirstOrDefault() ?? string.Empty,
            PurchasePlanCount = _dbContext.CrmVendorPurchasePlans.Count(plan => !plan.IsDeleted && plan.VendorId == vendor.Id),
            ProductCount = _dbContext.CrmBusinessEntityAttributes.Count(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == VendorEntityType &&
                attribute.EntityId == vendor.Id &&
                attribute.AttributeCode == PurchaseProductAttributeCode),
            ContactCount = _dbContext.CrmContacts.Count(contact =>
                !contact.IsDeleted &&
                contact.EntityType == VendorEntityType &&
                contact.EntityId == vendor.Id),
            CreatedAt = vendor.CreatedAt,
            UpdatedAt = vendor.UpdatedAt
        });

        var response = await dtoQuery.ToPaginationResponseAsync(request);
        await CrmVendorOwners.FillAsync(_dbContext, response.List, cancellationToken);
        return response;
    }
}


