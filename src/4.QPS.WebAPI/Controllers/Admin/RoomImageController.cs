using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.RoomImages;
using QPS.Application.Contracts.RoomImages;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/room-images")]
[Authorize]
public class RoomImageController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomImageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("room/{roomId}")]
    public async Task<ActionResult<List<RoomImageDto>>> GetRoomImages(Guid roomId)
    {
        var query = new GetRoomImagesQuery { RoomId = roomId };
        var images = await _mediator.Send(query);
        return Ok(images);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomImageDto>> GetRoomImage(Guid id)
    {
        var query = new GetRoomImageQuery { Id = id };
        var image = await _mediator.Send(query);
        return Ok(image);
    }

    [HttpPost]
    public async Task<ActionResult<RoomImageDto>> CreateRoomImage([FromBody] RoomImageCreateRequest request)
    {
        var command = new CreateRoomImageCommand { Request = request };
        var image = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRoomImage), new { id = image.Id }, image);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoomImageDto>> UpdateRoomImage(Guid id, [FromBody] RoomImageUpdateRequest request)
    {
        var command = new UpdateRoomImageCommand { Id = id, Request = request };
        var image = await _mediator.Send(command);
        return Ok(image);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteRoomImage(Guid id)
    {
        var command = new DeleteRoomImageCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}