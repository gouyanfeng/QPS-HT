using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class SetPrimaryCrmContactCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class SetPrimaryCrmContactHandler : IRequestHandler<SetPrimaryCrmContactCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;
    private const string VendorEntityType = CrmCodes.VendorEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public SetPrimaryCrmContactHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(SetPrimaryCrmContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await GetContact(request.Id, cancellationToken);
        EnsureContactCanBePrimary(contact);
        var subject = contact.EntityType == HerbBaseSubjectEntityType
            ? await GetSubject(contact, cancellationToken)
            : null;
        if (contact.EntityType == VendorEntityType)
        {
            await EnsureVendorExists(contact.EntityId, cancellationToken);
        }
        else if (subject == null)
        {
            throw new BusinessException(400, "不支持的联系人类型");
        }

        await UnmarkSiblingPrimaryContacts(contact, cancellationToken);
        contact.MarkPrimary();
        subject?.UpdatePrimaryContact(contact.ContactName, contact.Phone);

        await _dbContext.SaveChangesAsync(cancellationToken);
        if (subject != null)
        {
            await _publisher.Publish(new CrmHerbBaseSubjectScoreAffectedEvent(subject.Id), cancellationToken);
        }

        return true;
    }

    private async Task<CrmContact> GetContact(Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.CrmContacts.FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);
        if (contact == null)
        {
            throw new BusinessException(404, "联系人不存在");
        }

        return contact;
    }

    private static void EnsureContactCanBePrimary(CrmContact contact)
    {
        if (contact.Status == InvalidStatus)
        {
            throw new BusinessException(400, "无效联系人不能设为主联系人");
        }
    }

    private async Task<CrmHerbBaseSubject> GetSubject(CrmContact contact, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects.FirstOrDefaultAsync(
            item => item.Id == contact.EntityId &&
                contact.EntityType == HerbBaseSubjectEntityType,
            cancellationToken);

        if (subject == null)
        {
            throw new BusinessException(404, "药材基地主体不存在");
        }

        return subject;
    }

    private async Task EnsureVendorExists(Guid vendorId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.CrmVendors.AnyAsync(
            item => item.Id == vendorId && !item.IsDeleted,
            cancellationToken);

        if (!exists)
        {
            throw new BusinessException(404, "厂商不存在");
        }
    }

    private async Task UnmarkSiblingPrimaryContacts(CrmContact contact, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == contact.EntityType &&
                c.EntityId == contact.EntityId &&
                c.Id != contact.Id &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }
}
