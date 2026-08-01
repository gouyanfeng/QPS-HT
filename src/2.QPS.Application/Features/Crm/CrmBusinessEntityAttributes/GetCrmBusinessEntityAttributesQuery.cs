using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

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

    public GetCrmBusinessEntityAttributesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CrmBusinessEntityAttributeDto>> Handle(GetCrmBusinessEntityAttributesQuery request, CancellationToken cancellationToken)
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
}
