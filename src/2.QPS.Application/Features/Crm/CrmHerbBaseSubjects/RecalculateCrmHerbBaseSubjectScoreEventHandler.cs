using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Events.Crm;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public sealed class RecalculateCrmHerbBaseSubjectScoreEventHandler
    : INotificationHandler<CrmHerbBaseSubjectScoreAffectedEvent>
{
    private readonly IDbContext _dbContext;

    public RecalculateCrmHerbBaseSubjectScoreEventHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CrmHerbBaseSubjectScoreAffectedEvent notification, CancellationToken cancellationToken)
    {
        var scoreInput = await CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(
            _dbContext,
            notification.SubjectId,
            cancellationToken);

        if (scoreInput == null)
        {
            return;
        }

        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(item => item.Id == notification.SubjectId, cancellationToken);

        if (subject == null)
        {
            return;
        }

        subject.RecalculateScoreGrade(scoreInput);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
