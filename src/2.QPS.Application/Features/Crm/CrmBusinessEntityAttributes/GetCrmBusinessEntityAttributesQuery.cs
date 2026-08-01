using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmBusinessEntityAttributes;

public class GetCrmBusinessEntityAttributesQuery : IRequest<List<CrmBusinessEntityAttributeDto>>
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string? AttributeCode { get; set; }
}

public class GetCrmBusinessEntityAttributesHandler : IRequestHandler<GetCrmBusinessEntityAttributesQuery, List<CrmBusinessEntityAttributeDto>>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 查询 CRM 业务实体属性处理器。
    /// </summary>
    public GetCrmBusinessEntityAttributesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排查询业务实体属性用例。
    /// </summary>
    public async Task<List<CrmBusinessEntityAttributeDto>> Handle(GetCrmBusinessEntityAttributesQuery request, CancellationToken cancellationToken)
    {
        // 编排查询业务实体属性用例：
        // 构建查询条件、排序并映射 DTO。
        var query = BuildQuery(request);

        return await query
            .OrderBy(attribute => attribute.AttributeCode)
            .ThenBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .Select(attribute => new CrmBusinessEntityAttributeDto
            {
                Id = attribute.Id,
                EntityType = attribute.EntityType,
                EntityId = attribute.EntityId,
                AttributeCode = attribute.AttributeCode,
                AttributeValue = attribute.AttributeValue,
                SortOrder = attribute.SortOrder,
                Remark = attribute.Remark
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 构建业务实体属性查询。
    /// </summary>
    private IQueryable<CrmBusinessEntityAttribute> BuildQuery(GetCrmBusinessEntityAttributesQuery request)
    {
        var query = _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == request.EntityType &&
                attribute.EntityId == request.EntityId);

        if (!string.IsNullOrWhiteSpace(request.AttributeCode))
        {
            query = query.Where(attribute => attribute.AttributeCode == request.AttributeCode);
        }

        return query;
    }
}
