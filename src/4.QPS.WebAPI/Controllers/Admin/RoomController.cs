using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Rooms;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Pagination;

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
    public async Task<ActionResult<PaginationResponse<RoomDto>>> GetRooms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Name",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? roomNumber = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isEnabled = null)
    {
        var query = new GetRoomsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            RoomNumber = roomNumber,
            Status = status,
            IsEnabled = isEnabled
        };
        var rooms = await _mediator.Send(query);
        return Ok(rooms);
    }

    [HttpPost("toggle-power")]
    public async Task<ActionResult<bool>> TogglePower([FromBody] TogglePowerCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetRoom(Guid id)
    {
        var query = new GetRoomQuery { Id = id };
        var room = await _mediator.Send(query);
        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] RoomCreateRequest request)
    {
        var command = new CreateRoomCommand { Request = request };
        var room = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoomDto>> UpdateRoom(Guid id, [FromBody] RoomUpdateRequest request)
    {
        var command = new UpdateRoomCommand { Id = id, Request = request };
        var room = await _mediator.Send(command);
        return Ok(room);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteRoom(Guid id)
    {
        var command = new DeleteRoomCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}