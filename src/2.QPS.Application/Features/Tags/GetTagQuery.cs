using MediatR;
using QPS.Application.Contracts.Tags;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Tags;

public class GetTagQuery : IRequest<TagDto>
{
    public Guid Id { get; set; }
}

public class GetTagHandler : IRequestHandler<GetTagQuery, TagDto>
{
    private readonly IDbContext _dbContext;

    public GetTagHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagDto> Handle(GetTagQuery request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FindAsync(request.Id, cancellationToken);

        if (tag == null)
        {
            throw new BusinessException(404, "标签不存在");
        }

        return new TagDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            Category = tag.Category
        };
    }
}