using MediatR;
using QPS.Application.Contracts.RoomPlans;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.RoomPlans;

public class GetRoomPlansQuery : IRequest<List<RoomPlanDto>>
{
    public Guid? RoomId { get; set; }
    public Guid? PlanId { get; set; }
}

public class GetRoomPlansHandler : IRequestHandler<GetRoomPlansQuery, List<RoomPlanDto>>
{
    private readonly IDbContext _dbContext;

    public GetRoomPlansHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomPlanDto>> Handle(GetRoomPlansQuery request, CancellationToken cancellationToken)
    {
        var query = from rp in _dbContext.RoomPlans
                    join room in _dbContext.Rooms on rp.RoomId equals room.Id into roomJoin
                    from room in roomJoin.DefaultIfEmpty()
                    join plan in _dbContext.Plans on rp.PlanId equals plan.Id into planJoin
                    from plan in planJoin.DefaultIfEmpty()
                    select new RoomPlanDto
                    {
                        Id = rp.Id,
                        RoomId = rp.RoomId,
                        RoomNumber = room != null ? room.Name : null,
                        PlanId = rp.PlanId,
                        PlanName = plan != null ? plan.Name : null
                    };

        if (request.RoomId.HasValue)
        {
            query = query.Where(rp => rp.RoomId == request.RoomId.Value);
        }

        if (request.PlanId.HasValue)
        {
            query = query.Where(rp => rp.PlanId == request.PlanId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}