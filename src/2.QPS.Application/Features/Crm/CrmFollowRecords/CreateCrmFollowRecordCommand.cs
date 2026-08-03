using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmFollowRecords;

public class CreateCrmFollowRecordCommand : IRequest<bool>
{
    public Guid HerbBaseSubjectId { get; set; }

    public CrmFollowRecordCreateRequest Request { get; set; } = null!;
}

public class CreateCrmFollowRecordHandler : IRequestHandler<CreateCrmFollowRecordCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 创建基地主体沟通记录处理器。
    /// </summary>
    public CreateCrmFollowRecordHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 编排新增沟通记录用例。
    /// </summary>
    public async Task<bool> Handle(CreateCrmFollowRecordCommand request, CancellationToken cancellationToken)
    {
        // 编排新增沟通记录用例：
        // 校验沟通结果、确认主体与联系人、校验基地上下文、创建记录并同步主体跟进摘要。
        EnsureFollowResult(request.Request.FollowResult);

        var subject = await GetSubject(request.HerbBaseSubjectId, cancellationToken);

        await EnsureContactBelongsToSubject(request, cancellationToken);
        await EnsureHerbBaseBelongsToSubject(request, cancellationToken);

        var record = CreateFollowRecord(request);

        _dbContext.CrmFollowRecords.Add(record);
        
        subject.UpdateFollowSummary(DateTime.Now, request.Request.FollowResult, request.Request.NextFollowAt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 校验沟通结果。
    /// </summary>
    private static void EnsureFollowResult(string followResult)
    {
        if (string.IsNullOrWhiteSpace(followResult))
        {
            throw new BusinessException(400, "沟通结果不能为空");
        }
    }

    /// <summary>
    /// 获取沟通记录所属的药材基地主体。
    /// </summary>
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

    /// <summary>
    /// 确认联系人属于当前基地主体。
    /// </summary>
    private async Task EnsureContactBelongsToSubject(CreateCrmFollowRecordCommand command, CancellationToken cancellationToken)
    {
        if (!command.Request.ContactId.HasValue)
        {
            return;
        }

        var contact = await _dbContext.CrmContacts
            .FirstOrDefaultAsync(c => c.Id == command.Request.ContactId.Value, cancellationToken);

        if (contact == null ||
            contact.EntityType != HerbBaseSubjectEntityType ||
            contact.EntityId != command.HerbBaseSubjectId)
        {
            throw new BusinessException(404, "联系人不存在");
        }
    }

    /// <summary>
    /// 校验可选的基地上下文归属于当前主体。
    /// </summary>
    private async Task EnsureHerbBaseBelongsToSubject(
        CreateCrmFollowRecordCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Request.HerbBaseId.HasValue)
        {
            return;
        }

        var belongsToSubject = await _dbContext.CrmHerbBases.AnyAsync(
            herbBase => herbBase.Id == command.Request.HerbBaseId.Value &&
                        herbBase.HerbBaseSubjectId == command.HerbBaseSubjectId,
            cancellationToken);

        if (!belongsToSubject)
        {
            throw new BusinessException(400, "基地不属于当前主体");
        }
    }

    /// <summary>
    /// 根据请求创建沟通记录实体。
    /// </summary>
    private CrmFollowRecord CreateFollowRecord(CreateCrmFollowRecordCommand command)
    {
        var operatorUserId = GetOperatorUserId();

        return CrmFollowRecord.Create(
            command.HerbBaseSubjectId,
            command.Request.HerbBaseId,
            command.Request.ContactId,
            command.Request.FollowType,
            command.Request.FollowResult,
            command.Request.IntentLevel,
            command.Request.Content,
            command.Request.NextFollowAt,
            operatorUserId);
    }

    /// <summary>
    /// 获取当前操作人编号。
    /// </summary>
    private Guid? GetOperatorUserId()
    {
        return Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : null;
    }
}


