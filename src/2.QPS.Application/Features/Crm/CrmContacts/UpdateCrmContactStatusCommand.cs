using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
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
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    public UpdateCrmContactStatusHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateCrmContactStatusCommand request, CancellationToken cancellationToken)
    {
        var contact = await GetContact(request.Id, cancellationToken);
        var subject = await GetSubject(contact, cancellationToken);
        var wasPrimary = contact.IsPrimary;

        contact.MarkStatus(request.Request.Status, request.Request.Remark);
        await ApplyInvalidPrimaryContact(wasPrimary, contact, subject, cancellationToken);

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
}
