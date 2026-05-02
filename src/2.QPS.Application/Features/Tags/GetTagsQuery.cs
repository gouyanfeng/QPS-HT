using MediatR;
using QPS.Application.Contracts.Tags;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Tags;

public class GetTagsQuery : PaginationRequest, IRequest<PaginationResponse<TagDto>>
{
    public string? TagName { get; set; }
    public string? Category { get; set; }
}

public class GetTagsHandler : IRequestHandler<GetTagsQuery, PaginationResponse<TagDto>>
{
    private readonly IDbContext _dbContext;

    public GetTagsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tags.AsNoTracking();

        if (!string.IsNullOrEmpty(request.TagName))
        {
            query = query.Where(t => t.TagName.Contains(request.TagName));
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(t => t.Category.Contains(request.Category));
        }

        var dtoQuery = query.Select(t => new TagDto
        {
            Id = t.Id,
            TagName = t.TagName,
            Category = t.Category
        });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}