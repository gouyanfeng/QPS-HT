using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class UpdateCrmContactCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmContactUpdateRequest Request { get; set; } = null!;
}

public class UpdateCrmContactHandler : IRequestHandler<UpdateCrmContactCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    public UpdateCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateCrmContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await GetContact(request.Id, cancellationToken);
        EnsureCanSetPrimary(request.Request, contact);
        var subject = await GetSubject(contact, cancellationToken);
        var wasPrimary = contact.IsPrimary;

        await EnsurePhoneNotDuplicated(contact, request.Request.Phone, cancellationToken);
        UpdateContact(contact, request.Request);
        await ApplyPrimaryContactChange(subject, contact, wasPrimary, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var scoreInput = await CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(_dbContext, subject.Id, cancellationToken);
        if (scoreInput != null)
        {
            subject.RecalculateScoreGrade(scoreInput);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

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

    private static void EnsureCanSetPrimary(CrmContactUpdateRequest request, CrmContact contact)
    {
        if (request.IsPrimary && contact.Status == InvalidStatus)
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

    private async Task EnsurePhoneNotDuplicated(CrmContact contact, string phone, CancellationToken cancellationToken)
    {
        var normalizedPhone = phone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return;
        }

        var duplicated = await _dbContext.CrmContacts.AnyAsync(
            item =>
                item.EntityType == contact.EntityType &&
                item.EntityId == contact.EntityId &&
                item.Id != contact.Id &&
                item.Phone == normalizedPhone,
            cancellationToken);

        if (duplicated)
        {
            throw new BusinessException(400, "该主体下已存在相同联系电话");
        }
    }

    private static void UpdateContact(CrmContact contact, CrmContactUpdateRequest request)
    {
        contact.Update(
            request.ContactName,
            request.Phone,
            request.PhoneType,
            request.Wechat,
            request.RoleName,
            request.IsPrimary,
            request.Remark);
    }

    private async Task ApplyPrimaryContactChange(
        CrmHerbBaseSubject subject,
        CrmContact contact,
        bool wasPrimary,
        CancellationToken cancellationToken)
    {
        if (contact.IsPrimary)
        {
            await UnmarkSiblingPrimaryContacts(contact.EntityType, contact.EntityId, contact.Id, cancellationToken);
            subject.UpdatePrimaryContact(contact.ContactName, contact.Phone);
            return;
        }

        if (wasPrimary)
        {
            await PromoteOldestValidContactOrClear(subject, contact.Id, cancellationToken);
        }
    }

    private async Task UnmarkSiblingPrimaryContacts(string entityType, Guid entityId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == entityType &&
                c.EntityId == entityId &&
                c.Id != contactId &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }

    private async Task PromoteOldestValidContactOrClear(CrmHerbBaseSubject subject, Guid excludedContactId, CancellationToken cancellationToken)
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
            subject.ClearPrimaryContact();
            return;
        }

        replacement.MarkPrimary();
        subject.UpdatePrimaryContact(replacement.ContactName, replacement.Phone);
    }
}
