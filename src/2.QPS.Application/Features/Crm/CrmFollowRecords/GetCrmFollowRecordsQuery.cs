using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmFollowRecords;

public class GetCrmFollowRecordsQuery : IRequest<List<CrmFollowRecordDto>>
{
    public Guid HerbBaseSubjectId { get; set; }
}

public class GetCrmFollowRecordsHandler : IRequestHandler<GetCrmFollowRecordsQuery, List<CrmFollowRecordDto>>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 查询客户沟通记录处理器。
    /// </summary>
    public GetCrmFollowRecordsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排查询客户沟通记录用例。
    /// </summary>
    public async Task<List<CrmFollowRecordDto>> Handle(GetCrmFollowRecordsQuery request, CancellationToken cancellationToken)
    {
        // 编排查询客户沟通记录用例：
        // 按基地主体编号过滤、加载联系人、按创建时间倒序映射 DTO。
        return await _dbContext.CrmFollowRecords
            .Include(r => r.Contact)
            .Where(r => r.HerbBaseSubjectId == request.HerbBaseSubjectId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CrmFollowRecordDto
            {
                Id = r.Id,
                HerbBaseSubjectId = r.HerbBaseSubjectId,
                HerbBaseId = r.HerbBaseId,
                ContactId = r.ContactId,
                ContactName = r.Contact != null ? r.Contact.ContactName : null,
                FollowType = r.FollowType,
                FollowResult = r.FollowResult,
                IntentLevel = r.IntentLevel,
                Content = r.Content,
                NextFollowAt = r.NextFollowAt,
                OperatorUserId = r.OperatorUserId,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
