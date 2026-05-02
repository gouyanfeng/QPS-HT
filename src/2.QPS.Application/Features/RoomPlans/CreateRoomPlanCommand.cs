using MediatR;
using QPS.Application.Contracts.RoomPlans;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.RoomPlans;

public class CreateRoomPlanCommand : IRequest<RoomPlanDto>
{
    public Guid RoomId { get; set; }
    public Guid PlanId { get; set; }
}

public class CreateRoomPlanHandler : IRequestHandler<CreateRoomPlanCommand, RoomPlanDto>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoomPlanHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<RoomPlanDto> Handle(CreateRoomPlanCommand request, CancellationToken cancellationToken)
    {
        var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);
        if (!roomExists)
        {
            throw new BusinessException(404, "房间不存在");
        }

        var planExists = await _dbContext.Plans.AnyAsync(p => p.Id == request.PlanId, cancellationToken);
        if (!planExists)
        {
            throw new BusinessException(404, "套餐不存在");
        }

        var existingMapping = await _dbContext.RoomPlans
            .AnyAsync(rp => rp.RoomId == request.RoomId && rp.PlanId == request.PlanId, cancellationToken);
        if (existingMapping)
        {
            throw new BusinessException(400, "该房间已关联此套餐");
        }

        var mapping = new RoomPlan(request.RoomId, request.PlanId);
        mapping.GetType().GetProperty("MerchantId")?.SetValue(mapping, _currentUserService.MerchantId);

        _dbContext.RoomPlans.Add(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var room = await _dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);
        var plan = await _dbContext.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        return new RoomPlanDto
        {
            Id = mapping.Id,
            RoomId = mapping.RoomId,
            RoomNumber = room != null ? room.Name : null,
            PlanId = mapping.PlanId,
            PlanName = plan != null ? plan.Name : null
        };
    }
}