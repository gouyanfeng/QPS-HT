using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.RoomTags;
using QPS.Application.Contracts.RoomTags;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/room-tags")]
[Authorize]
public class RoomTagController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomTagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoomTagDto>>> GetRoomTags(
        [FromQuery] Guid? roomId = null,
        [FromQuery] Guid? tagId = null)
    {
        var mappings = await _mediator.Send(new GetRoomTagsQuery { RoomId = roomId, TagId = tagId });
        return Ok(mappings);
    }

    [HttpPost]
    public async Task<ActionResult<RoomTagDto>> CreateRoomTag([FromBody] RoomTagRequest request)
    {
        var mapping = await _mediator.Send(new CreateRoomTagCommand { RoomId = request.RoomId, TagId = request.TagId });
        return CreatedAtAction(nameof(GetRoomTags), new { roomId = mapping.RoomId }, mapping);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteRoomTag(Guid id)
    {
        var result = await _mediator.Send(new DeleteRoomTagCommand { Id = id });
        return Ok(result);
    }
}