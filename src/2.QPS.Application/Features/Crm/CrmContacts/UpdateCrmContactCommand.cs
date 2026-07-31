using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class UpdateCrmContactCommand : IRequest<CrmContactDto>
{
    public Guid Id { get; set; }

    public CrmContactUpdateRequest Request { get; set; } = null!;
}

public class UpdateCrmContactHandler : IRequestHandler<UpdateCrmContactCommand, CrmContactDto>
{
    private const string CustomerEntityType = "CRM_HERB_BASE";
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    public UpdateCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmContactDto> Handle(UpdateCrmContactCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.ContactName) &&
            string.IsNullOrWhiteSpace(request.Request.Phone))
        {
            throw new BusinessException(400, "联系人姓名和电话至少填写一项");
        }

        var contact = await _dbContext.CrmContacts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contact == null)
        {
            throw new BusinessException(404, "联系人不存在");
        }

        if (request.Request.IsPrimary && contact.Status == InvalidStatus)
        {
            throw new BusinessException(400, "无效联系人不能设为主联系人");
        }

        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == contact.EntityId && contact.EntityType == CustomerEntityType && !c.IsDeleted, cancellationToken);
        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        var wasPrimary = contact.IsPrimary;

        contact.Update(
            request.Request.ContactName,
            request.Request.Phone,
            request.Request.PhoneType,
            request.Request.Wechat,
            request.Request.RoleName,
            request.Request.IsPrimary,
            request.Request.Remark);

        if (contact.IsPrimary)
        {
            await UnmarkSiblingPrimaryContacts(contact.EntityType, contact.EntityId, contact.Id, cancellationToken);
            customer.UpdatePrimaryContact(contact.ContactName, contact.Phone);
        }
        else if (wasPrimary)
        {
            await PromoteOldestValidContactOrClear(customer, contact.Id, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(contact);
    }

    private async Task UnmarkSiblingPrimaryContacts(string entityType, Guid entityId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c => c.EntityType == entityType && c.EntityId == entityId && c.Id != contactId && c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }

    private async Task PromoteOldestValidContactOrClear(CrmHerbBase customer, Guid excludedContactId, CancellationToken cancellationToken)
    {
        var replacement = await _dbContext.CrmContacts
            .Where(c => c.EntityType == CustomerEntityType && c.EntityId == customer.Id && c.Id != excludedContactId && c.Status != InvalidStatus)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (replacement == null)
        {
            customer.ClearPrimaryContact();
            return;
        }

        replacement.MarkPrimary();
        customer.UpdatePrimaryContact(replacement.ContactName, replacement.Phone);
    }

    private static CrmContactDto MapToDto(CrmContact contact)
    {
        return new CrmContactDto
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
        };
    }
}


