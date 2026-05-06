using MediatR;
using QPS.Application.Contracts.Tags;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Tags;

public class UpdateTagCommand : IRequest<TagDto>
{
    public Guid Id { get; set; }
    public TagUpdateRequest Request { get; set; }
}

public class UpdateTagHandler : IRequestHandler<UpdateTagCommand, TagDto>
{
    private readonly IDbContext _dbContext;

    public UpdateTagHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FindAsync(request.Id, cancellationToken);

        if (tag == null)
        {
            throw new BusinessException(404, "标签不存在");
        }

        tag.Update(request.Request.TagName, request.Request.Category);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            Category = tag.Category,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }
}