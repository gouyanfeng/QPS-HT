using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.RoomPlans;
using QPS.Application.Contracts.RoomPlans;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/room-plans")]
[Authorize]
public class RoomPlanController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomPlanController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<RoomPlanDto>>> GetRoomPlans(
        [FromQuery] Guid? roomId = null,
        [FromQuery] Guid? planId = null)
    {
        var mappings = await _mediator.Send(new GetRoomPlansQuery { RoomId = roomId, PlanId = planId });
        return Ok(mappings);
    }

    [HttpPost]
    public async Task<ActionResult<RoomPlanDto>> CreateRoomPlan([FromBody] RoomPlanRequest request)
    {
        var mapping = await _mediator.Send(new CreateRoomPlanCommand { RoomId = request.RoomId, PlanId = request.PlanId });
        return CreatedAtAction(nameof(GetRoomPlans), new { roomId = mapping.RoomId }, mapping);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteRoomPlan(Guid id)
    {
        var result = await _mediator.Send(new DeleteRoomPlanCommand { Id = id });
        return Ok(result);
    }
}