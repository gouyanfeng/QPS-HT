using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Tags;

public class DeleteTagCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteTagHandler : IRequestHandler<DeleteTagCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeleteTagHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FindAsync(request.Id, cancellationToken);

        if (tag == null)
        {
            throw new BusinessException(404, "标签不存在");
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}