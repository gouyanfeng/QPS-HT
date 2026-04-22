using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Rooms;
using QPS.Application.Contracts.Rooms;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/rooms")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> GetRooms()
    {
        var rooms = await _mediator.Send(new GetRoomsQuery());
        return Ok(rooms);
    }

    [HttpPost("toggle-power")]
    public async Task<ActionResult<bool>> TogglePower([FromBody] TogglePowerCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}