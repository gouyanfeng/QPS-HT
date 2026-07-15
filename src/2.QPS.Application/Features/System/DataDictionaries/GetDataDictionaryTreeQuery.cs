using Microsoft.EntityFrameworkCore;
using MediatR;
using QPS.Application.Contracts.System.DataDictionaries;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.System.DataDictionaries;

public record GetDataDictionaryTreeQuery : IRequest<List<DataDictionaryDto>>
{
}

public class GetDataDictionaryTreeQueryHandler : IRequestHandler<GetDataDictionaryTreeQuery, List<DataDictionaryDto>>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetDataDictionaryTreeQueryHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<DataDictionaryDto>> Handle(GetDataDictionaryTreeQuery request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;

        var allDictionaries = await _dbContext.SystemDataDictionaries
            .Where(d => d.MerchantId == merchantId && d.IsActive)
            .OrderBy(d => d.SortOrder)
            .ToListAsync(cancellationToken);

        var rootNodes = allDictionaries.Where(d => !d.ParentId.HasValue).ToList();

        return BuildTree(rootNodes, allDictionaries);
    }

    private List<DataDictionaryDto> BuildTree(List<Domain.Entities.System.SystemDataDictionary> parents,
        List<Domain.Entities.System.SystemDataDictionary> allNodes)
    {
        var result = new List<DataDictionaryDto>();

        foreach (var parent in parents)
        {
            var children = allNodes.Where(n => n.ParentId == parent.Id).ToList();

            var dto = new DataDictionaryDto
            {
                Id = parent.Id,
                ParentId = parent.ParentId,
                Code = parent.Code,
                Name = parent.Name,
                Value = parent.Value,
                Description = parent.Description,
                SortOrder = parent.SortOrder,
                IsActive = parent.IsActive,
                MerchantId = parent.MerchantId,
                Children = BuildTree(children, allNodes)
            };

            result.Add(dto);
        }

        return result;
    }
}