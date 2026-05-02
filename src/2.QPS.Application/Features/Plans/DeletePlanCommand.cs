using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Plans;

public class DeletePlanCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeletePlanHandler : IRequestHandler<DeletePlanCommand, bool>
{
    private readonly IDbContext _dbContext;

    public DeletePlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.Plans.FindAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            throw new BusinessException(404, "套餐不存在");
        }

        _dbContext.Plans.Remove(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}