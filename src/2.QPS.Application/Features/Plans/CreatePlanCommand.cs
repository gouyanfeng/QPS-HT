using MediatR;
using QPS.Application.Contracts.Plans;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Plans;

public class CreatePlanCommand : IRequest<PlanDto>
{
    public PlanCreateRequest Request { get; set; }
}

public class CreatePlanHandler : IRequestHandler<CreatePlanCommand, PlanDto>
{
    private readonly IDbContext _dbContext;

    public CreatePlanHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlanDto> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = new Plan(
            request.Request.Name,
            request.Request.Description,
            request.Request.Price,
            request.Request.DurationMinutes
        );

        _dbContext.Plans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

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