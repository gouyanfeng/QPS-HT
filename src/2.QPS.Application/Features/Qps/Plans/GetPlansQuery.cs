using MediatR;
using QPS.Application.Contracts.Qps.Plans;
using QPS.Application.Interfaces;
using QPS.Application.Extensions;
using QPS.Domain.Entities;
using QPS.Domain.Entities.Qps;using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Qps.Plans;

public class GetPlansQuery : PaginationRequest, IRequest<PaginationResponse<PlanDto>>
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public class GetPlansHandler : IRequestHandler<GetPlansQuery, PaginationResponse<PlanDto>>
{
    private readonly IDbContext _dbContext;

    public GetPlansHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Plans.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(p => p.Name.Contains(request.Name));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var dtoQuery = query.Select(p => new PlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DurationMinutes = p.DurationMinutes,
            IsActive = p.IsActive
        });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}