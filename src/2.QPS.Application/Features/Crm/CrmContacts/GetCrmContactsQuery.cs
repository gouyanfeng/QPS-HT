using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmContacts;

public class GetCrmContactsQuery : IRequest<List<CrmContactDto>>
{
    public Guid HerbBaseSubjectId { get; set; }
}

public class GetCrmContactsHandler : IRequestHandler<GetCrmContactsQuery, List<CrmContactDto>>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;

    private readonly IDbContext _dbContext;

    /// <summary>
    /// 查询基地主体联系人处理器。
    /// </summary>
    public GetCrmContactsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排查询客户联系人用例。
    /// </summary>
    public async Task<List<CrmContactDto>> Handle(GetCrmContactsQuery request, CancellationToken cancellationToken)
    {
        // 编排查询客户联系人用例：
        // 按基地主体编号过滤、主联系人优先排序并映射 DTO。
        return await _dbContext.CrmContacts
            .Where(c => c.EntityType == HerbBaseSubjectEntityType && c.EntityId == request.HerbBaseSubjectId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.CreatedAt)
            .Select(c => new CrmContactDto
            {
                Id = c.Id,
                EntityType = c.EntityType,
                EntityId = c.EntityId,
                ContactName = c.ContactName,
                Phone = c.Phone,
                PhoneType = c.PhoneType,
                Wechat = c.Wechat,
                RoleName = c.RoleName,
                IsPrimary = c.IsPrimary,
                Status = c.Status,
                Remark = c.Remark,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
