using MediatR;
using QPS.Application.Contracts.Tags;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Tags;

public class CreateTagCommand : IRequest<TagDto>
{
    public TagCreateRequest Request { get; set; }
}

public class CreateTagHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IDbContext _dbContext;

    public CreateTagHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = new Tag(request.Request.TagName);

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }
}