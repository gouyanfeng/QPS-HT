using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public class UpdateCrmHerbBaseSubjectCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmHerbBaseSubjectUpdateRequest Request { get; set; } = null!;
}

public class UpdateCrmHerbBaseSubjectHandler : IRequestHandler<UpdateCrmHerbBaseSubjectCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public UpdateCrmHerbBaseSubjectHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(UpdateCrmHerbBaseSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (subject == null)
            throw new BusinessException(404, "药材基地主体不存在");

        subject.UpdateBasicInfo(
            request.Request.SubjectName,
            request.Request.SubjectType,
            request.Request.Status,
            request.Request.Grade,
            request.Request.Score,
            request.Request.Remark);

        var syncedSubjectName = subject.SubjectName ?? string.Empty;
        var herbBases = await _dbContext.CrmHerbBases
            .Where(item => item.HerbBaseSubjectId == subject.Id)
            .ToListAsync(cancellationToken);

        foreach (var herbBase in herbBases)
        {
            herbBase.RenameSubject(syncedSubjectName);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new CrmHerbBaseSubjectScoreAffectedEvent(subject.Id), cancellationToken);
        return true;
    }
}
