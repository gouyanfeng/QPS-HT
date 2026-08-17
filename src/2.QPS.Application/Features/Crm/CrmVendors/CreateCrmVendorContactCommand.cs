using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class CreateCrmVendorContactCommand : IRequest<bool>
{
    public Guid VendorId { get; set; }

    public CrmContactCreateRequest Request { get; set; } = null!;
}

public class CreateCrmVendorContactHandler : IRequestHandler<CreateCrmVendorContactCommand, bool>
{
    private const string VendorEntityType = CrmCodes.VendorEntityType;

    private readonly IDbContext _dbContext;

    public CreateCrmVendorContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(CreateCrmVendorContactCommand request, CancellationToken cancellationToken)
    {
        var vendorExists = await _dbContext.CrmVendors
            .AnyAsync(vendor => vendor.Id == request.VendorId && !vendor.IsDeleted, cancellationToken);
        if (!vendorExists)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        await EnsurePhoneNotDuplicated(request.VendorId, request.Request.Phone, cancellationToken);
        var shouldBePrimary = request.Request.IsPrimary || !await HasContact(request.VendorId, cancellationToken);
        var contact = CrmContact.Create(
            VendorEntityType,
            request.VendorId,
            request.Request.ContactName,
            request.Request.Phone,
            request.Request.PhoneType,
            request.Request.Wechat,
            request.Request.RoleName,
            shouldBePrimary,
            request.Request.Remark);

        if (shouldBePrimary)
        {
            await UnmarkSiblingPrimaryContacts(request.VendorId, contact.Id, cancellationToken);
        }

        _dbContext.CrmContacts.Add(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> HasContact(Guid vendorId, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmContacts.AnyAsync(
            contact =>
                !contact.IsDeleted &&
                contact.EntityType == VendorEntityType &&
                contact.EntityId == vendorId,
            cancellationToken);
    }

    private async Task EnsurePhoneNotDuplicated(Guid vendorId, string phone, CancellationToken cancellationToken)
    {
        var normalizedPhone = phone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return;
        }

        var exists = await _dbContext.CrmContacts.AnyAsync(
            contact =>
                !contact.IsDeleted &&
                contact.EntityType == VendorEntityType &&
                contact.EntityId == vendorId &&
                contact.Phone == normalizedPhone,
            cancellationToken);

        if (exists)
        {
            throw new BusinessException(400, "该厂商下已存在相同联系电话");
        }
    }

    private async Task UnmarkSiblingPrimaryContacts(Guid vendorId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(contact =>
                !contact.IsDeleted &&
                contact.EntityType == VendorEntityType &&
                contact.EntityId == vendorId &&
                contact.Id != contactId &&
                contact.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }
}
