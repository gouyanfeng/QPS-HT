using MediatR;
using QPS.Application.Contracts.Plans;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Plans;

public class GetPlanQuery : IRequest<PlanDto>
{
    public Guid Id { get; set; }
}

public class GetPlanHandler : IRequestHandler<GetPlanQuery, PlanDto>
{
    private readonly IDbContext _dbContext;

    public GetPlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlanDto> Handle(GetPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.Plans.FindAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            throw new BusinessException(404, "套餐不存在");
        }

        return new PlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            DurationMinutes = plan.DurationMinutes,
            IsActive = plan.IsActive
        };
    }
}