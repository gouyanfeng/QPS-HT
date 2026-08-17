using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class UpdateCrmContactStatusCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmContactStatusRequest Request { get; set; } = null!;
}

public class UpdateCrmContactStatusHandler : IRequestHandler<UpdateCrmContactStatusCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;
    private const string VendorEntityType = CrmCodes.VendorEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public UpdateCrmContactStatusHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(UpdateCrmContactStatusCommand request, CancellationToken cancellationToken)
    {
        var contact = await GetContact(request.Id, cancellationToken);
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

        var wasPrimary = contact.IsPrimary;

        contact.MarkStatus(request.Request.Status, request.Request.Remark);
        if (subject != null)
        {
            await ApplyInvalidPrimaryContact(wasPrimary, contact, subject, cancellationToken);
        }
        else
        {
            await ApplyInvalidVendorPrimaryContact(wasPrimary, contact, cancellationToken);
        }

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

    private async Task ApplyInvalidPrimaryContact(
        bool wasPrimary,
        CrmContact contact,
        CrmHerbBaseSubject subject,
        CancellationToken cancellationToken)
    {
        if (!wasPrimary || contact.Status != InvalidStatus)
        {
            return;
        }

        contact.UnmarkPrimary();
        subject.ClearPrimaryContact();

        await PromoteOldestValidContact(subject, contact.Id, cancellationToken);
    }

    private async Task PromoteOldestValidContact(CrmHerbBaseSubject subject, Guid excludedContactId, CancellationToken cancellationToken)
    {
        var replacement = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == HerbBaseSubjectEntityType &&
                c.EntityId == subject.Id &&
                c.Id != excludedContactId &&
                c.Status != InvalidStatus)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (replacement == null)
        {
            return;
        }

        replacement.MarkPrimary();
        subject.UpdatePrimaryContact(replacement.ContactName, replacement.Phone);
    }

    private async Task ApplyInvalidVendorPrimaryContact(
        bool wasPrimary,
        CrmContact contact,
        CancellationToken cancellationToken)
    {
        if (!wasPrimary || contact.Status != InvalidStatus)
        {
            return;
        }

        contact.UnmarkPrimary();

        var replacement = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == VendorEntityType &&
                c.EntityId == contact.EntityId &&
                c.Id != contact.Id &&
                c.Status != InvalidStatus)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        replacement?.MarkPrimary();
    }
}
