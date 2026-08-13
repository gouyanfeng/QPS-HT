using MediatR;

namespace QPS.Domain.Events.Crm;

public sealed class CrmHerbBaseSubjectScoreAffectedEvent : INotification
{
    public CrmHerbBaseSubjectScoreAffectedEvent(Guid subjectId)
    {
        SubjectId = subjectId;
    }

    public Guid SubjectId { get; }
}
