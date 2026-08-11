using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmBusinessEntityAttributes;

public class GetCrmBusinessEntityAttributeOptionsQuery : IRequest<List<AttributeOptionDto>>
{
    public string EntityType { get; set; } = string.Empty;

    public string AttributeCode { get; set; } = string.Empty;

    public string? Keyword { get; set; }

    public int PageSize { get; set; } = 100;
}

public class GetCrmBusinessEntityAttributeOptionsHandler : IRequestHandler<GetCrmBusinessEntityAttributeOptionsQuery, List<AttributeOptionDto>>
{
    private readonly IDbContext _dbContext;

    public GetCrmBusinessEntityAttributeOptionsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AttributeOptionDto>> Handle(GetCrmBusinessEntityAttributeOptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.CrmBusinessEntityAttributes
            .AsNoTracking()
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == request.EntityType &&
                attribute.AttributeCode == request.AttributeCode &&
                attribute.AttributeValue != string.Empty);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(attribute => attribute.AttributeValue.Contains(request.Keyword));
        }

        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        return await query
            .GroupBy(attribute => attribute.AttributeValue)
            .Select(group => new
            {
                Value = group.Key,
                SortOrder = group.Min(attribute => attribute.SortOrder),
                CreatedAt = group.Min(attribute => attribute.CreatedAt)
            })
            .OrderBy(option => option.SortOrder)
            .ThenBy(option => option.CreatedAt)
            .ThenBy(option => option.Value)
            .Take(pageSize)
            .Select(option => new AttributeOptionDto
            {
                Label = option.Value,
                Value = option.Value
            })
            .ToListAsync(cancellationToken);
    }
}
