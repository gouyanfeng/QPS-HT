using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class CreateCrmContactCommand : IRequest<bool>
{
    public Guid HerbBaseSubjectId { get; set; }

    public CrmContactCreateRequest Request { get; set; } = null!;
}

public class CreateCrmContactHandler : IRequestHandler<CreateCrmContactCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;

    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public CreateCrmContactHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CreateCrmContactCommand request, CancellationToken cancellationToken)
    {
        var subject = await GetSubject(request.HerbBaseSubjectId, cancellationToken);
        await EnsurePhoneNotDuplicated(request.HerbBaseSubjectId, request.Request.Phone, cancellationToken);
        var contact = CreateContact(request, subject);
        await ApplyPrimaryContact(subject, contact, cancellationToken);

        _dbContext.CrmContacts.Add(contact);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new CrmHerbBaseSubjectScoreAffectedEvent(subject.Id), cancellationToken);

        return true;
    }

    private async Task<CrmHerbBaseSubject> GetSubject(Guid herbBaseSubjectId, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(subject => subject.Id == herbBaseSubjectId, cancellationToken);

        if (subject == null)
        {
            throw new BusinessException(404, "药材基地主体不存在");
        }

        return subject;
    }

    private async Task EnsurePhoneNotDuplicated(Guid herbBaseSubjectId, string phone, CancellationToken cancellationToken)
    {
        var normalizedPhone = phone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return;
        }

        var exists = await _dbContext.CrmContacts.AnyAsync(
            contact =>
                contact.EntityType == HerbBaseSubjectEntityType &&
                contact.EntityId == herbBaseSubjectId &&
                contact.Phone == normalizedPhone,
            cancellationToken);

        if (exists)
        {
            throw new BusinessException(400, "该主体下已存在相同联系电话");
        }
    }

    private static CrmContact CreateContact(CreateCrmContactCommand command, CrmHerbBaseSubject subject)
    {
        var shouldBePrimary = command.Request.IsPrimary ||
            (
                string.IsNullOrWhiteSpace(subject.PrimaryContactName) &&
                string.IsNullOrWhiteSpace(subject.PrimaryContactPhone));

        return CrmContact.Create(
            HerbBaseSubjectEntityType,
            command.HerbBaseSubjectId,
            command.Request.ContactName,
            command.Request.Phone,
            command.Request.PhoneType,
            command.Request.Wechat,
            command.Request.RoleName,
            shouldBePrimary,
            command.Request.Remark);
    }

    private async Task ApplyPrimaryContact(CrmHerbBaseSubject subject, CrmContact contact, CancellationToken cancellationToken)
    {
        if (!contact.IsPrimary)
        {
            return;
        }

        await UnmarkSiblingPrimaryContacts(subject.Id, contact.Id, cancellationToken);
        subject.UpdatePrimaryContact(contact.ContactName, contact.Phone);
    }

    private async Task UnmarkSiblingPrimaryContacts(Guid herbBaseSubjectId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == HerbBaseSubjectEntityType &&
                c.EntityId == herbBaseSubjectId &&
                c.Id != contactId &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }
}
