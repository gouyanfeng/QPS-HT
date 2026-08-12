using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class SetPrimaryCrmContactCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class SetPrimaryCrmContactHandler : IRequestHandler<SetPrimaryCrmContactCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    public SetPrimaryCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(SetPrimaryCrmContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await GetContact(request.Id, cancellationToken);
        EnsureContactCanBePrimary(contact);
        var subject = await GetSubject(contact, cancellationToken);

        await UnmarkSiblingPrimaryContacts(contact, cancellationToken);
        contact.MarkPrimary();
        subject.UpdatePrimaryContact(contact.ContactName, contact.Phone);

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
