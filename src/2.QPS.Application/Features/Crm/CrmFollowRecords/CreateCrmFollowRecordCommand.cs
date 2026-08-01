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
    public Guid CustomerId { get; set; }

    public CrmFollowRecordCreateRequest Request { get; set; } = null!;
}

public class CreateCrmFollowRecordHandler : IRequestHandler<CreateCrmFollowRecordCommand, bool>
{
    private const string CustomerEntityType = CrmCodes.HerbBaseEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 创建客户沟通记录处理器。
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
        // 校验沟通结果、确认客户与联系人、创建记录、同步客户跟进摘要。
        EnsureFollowResult(request.Request.FollowResult);

        var customer = await GetCustomer(request.CustomerId, cancellationToken);

        await EnsureContactBelongsToCustomer(request, cancellationToken);

        var record = CreateFollowRecord(request);

        _dbContext.CrmFollowRecords.Add(record);
        
        customer.UpdateFollowSummary(DateTime.Now, request.Request.FollowResult, request.Request.NextFollowAt);

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
    /// 获取沟通记录所属客户。
    /// </summary>
    private async Task<CrmHerbBase> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        return customer;
    }

    /// <summary>
    /// 确认联系人属于当前客户。
    /// </summary>
    private async Task EnsureContactBelongsToCustomer(CreateCrmFollowRecordCommand command, CancellationToken cancellationToken)
    {
        if (!command.Request.ContactId.HasValue)
        {
            return;
        }

        var contact = await _dbContext.CrmContacts
            .FirstOrDefaultAsync(c => c.Id == command.Request.ContactId.Value, cancellationToken);

        if (contact == null ||
            contact.EntityType != CustomerEntityType ||
            contact.EntityId != command.CustomerId)
        {
            throw new BusinessException(404, "联系人不存在");
        }
    }

    /// <summary>
    /// 根据请求创建沟通记录实体。
    /// </summary>
    private CrmFollowRecord CreateFollowRecord(CreateCrmFollowRecordCommand command)
    {
        var operatorUserId = GetOperatorUserId();

        return CrmFollowRecord.Create(
            command.CustomerId,
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


