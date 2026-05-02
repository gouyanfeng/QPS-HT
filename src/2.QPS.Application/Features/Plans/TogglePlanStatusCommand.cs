using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Plans;

public class TogglePlanStatusCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}

public class TogglePlanStatusHandler : IRequestHandler<TogglePlanStatusCommand, bool>
{
    private readonly IDbContext _dbContext;

    public TogglePlanStatusHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(TogglePlanStatusCommand request, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.Plans.FindAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            throw new BusinessException(404, "套餐不存在");
        }

        if (request.IsActive)
        {
            plan.Activate();
        }
        else
        {
            plan.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return plan.IsActive;
    }
}