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
}