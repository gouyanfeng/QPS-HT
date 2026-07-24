using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.DataDictionaries;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.System;

namespace QPS.Application.Features.System.DataDictionaries;

public class GetDataDictionariesQuery : PaginationRequest, IRequest<PaginationResponse<DataDictionaryDto>>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentId { get; set; }
}

public class GetDataDictionariesQueryHandler : IRequestHandler<GetDataDictionariesQuery, PaginationResponse<DataDictionaryDto>>
{
    private readonly IDbContext _dbContext;

    public GetDataDictionariesQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<DataDictionaryDto>> Handle(GetDataDictionariesQuery request, CancellationToken cancellationToken)
    {
        var allDictionaries = await _dbContext.SystemDataDictionaries
            .OrderBy(d => d.SortOrder)
            .ToListAsync(cancellationToken);

        var filteredDictionaries = allDictionaries.AsQueryable();

        if (!string.IsNullOrEmpty(request.Code))
        {
            filteredDictionaries = filteredDictionaries.Where(d => d.Code.Contains(request.Code));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            filteredDictionaries = filteredDictionaries.Where(d => d.Name.Contains(request.Name));
        }

        if (request.IsActive.HasValue)
        {
            filteredDictionaries = filteredDictionaries.Where(d => d.IsActive == request.IsActive.Value);
        }

        if (request.ParentId.HasValue)
        {
            filteredDictionaries = filteredDictionaries.Where(d =>
                IsDescendantOf(d.Id, request.ParentId.Value, allDictionaries));
        }

        var dtoQuery = filteredDictionaries
            .Select(d => new DataDictionaryDto
            {
                Id = d.Id,
                ParentId = d.ParentId,
                ParentName = d.ParentId.HasValue
                    ? GetParentName(allDictionaries, d.ParentId.Value)
                    : null,
                Code = d.Code,
                Name = d.Name,
                Value = d.Value,
                Description = d.Description,
                SortOrder = d.SortOrder,
                IsActive = d.IsActive
            });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }

    private static bool IsDescendantOf(Guid nodeId, Guid parentId, IReadOnlyList<SystemDataDictionary> dictionaries)
    {
        var dictionaryById = dictionaries.ToDictionary(d => d.Id);
        var current = dictionaryById.GetValueOrDefault(nodeId);

        while (current != null)
        {
            if (current.ParentId == parentId)
            {
                return true;
            }

            if (!current.ParentId.HasValue)
            {
                return false;
            }

            current = dictionaryById.GetValueOrDefault(current.ParentId.Value);
        }

        return false;
    }

    private static string? GetParentName(IReadOnlyList<SystemDataDictionary> dictionaries, Guid parentId)
    {
        var parent = dictionaries.FirstOrDefault(x => x.Id == parentId);
        return parent?.Name;
    }
}
