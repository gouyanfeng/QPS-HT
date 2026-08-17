using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmFollowRecords;

public class CreateCrmFollowRecordCommand : IRequest<bool>
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public CrmFollowRecordCreateRequest Request { get; set; } = null!;
}

public class CreateCrmFollowRecordHandler : IRequestHandler<CreateCrmFollowRecordCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public CreateCrmFollowRecordHandler(IDbContext dbContext, ICurrentUserService currentUserService, IPublisher publisher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CreateCrmFollowRecordCommand request, CancellationToken cancellationToken)
    {
        EnsureFollowResult(request.Request.FollowResult);
        EnsureSupportedEntityType(request.EntityType);
        var subject = await GetSubject(request.EntityId, cancellationToken);

        await EnsureContactBelongsToSubject(request, cancellationToken);

        var record = CreateFollowRecord(request);
        _dbContext.CrmFollowRecords.Add(record);

        subject.UpdateFollowSummary(DateTime.Now, request.Request.FollowResult, request.Request.NextFollowAt);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new CrmHerbBaseSubjectScoreAffectedEvent(subject.Id), cancellationToken);

        return true;
    }

    private static void EnsureSupportedEntityType(string entityType)
    {
        if (entityType != HerbBaseSubjectEntityType)
        {
            throw new BusinessException(400, "不支持的沟通记录对象类型");
        }
    }

    private static void EnsureFollowResult(string followResult)
    {
        if (string.IsNullOrWhiteSpace(followResult))
        {
            throw new BusinessException(400, "沟通结果不能为空");
        }
    }

    private async Task<CrmHerbBaseSubject> GetSubject(Guid herbBaseSubjectId, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects.FirstOrDefaultAsync(subject => subject.Id == herbBaseSubjectId, cancellationToken);
        if (subject == null)
        {
            throw new BusinessException(404, "药材基地主体不存在");
        }

        return subject;
    }

    private async Task EnsureContactBelongsToSubject(CreateCrmFollowRecordCommand command, CancellationToken cancellationToken)
    {
        if (!command.Request.ContactId.HasValue)
        {
            return;
        }

        var contact = await _dbContext.CrmContacts.FirstOrDefaultAsync(c => c.Id == command.Request.ContactId.Value, cancellationToken);
        if (contact == null ||
            contact.EntityType != command.EntityType ||
            contact.EntityId != command.EntityId)
        {
            throw new BusinessException(404, "联系人不存在");
        }
    }

    private CrmFollowRecord CreateFollowRecord(CreateCrmFollowRecordCommand command)
    {
        var operatorUserId = GetOperatorUserId();

        return CrmFollowRecord.Create(
            command.EntityType,
            command.EntityId,
            command.Request.ContactId,
            command.Request.FollowType,
            command.Request.FollowResult,
            command.Request.IntentLevel,
            command.Request.Content,
            command.Request.NextFollowAt,
            operatorUserId);
    }

    private Guid? GetOperatorUserId()
    {
        return Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : null;
    }
}
