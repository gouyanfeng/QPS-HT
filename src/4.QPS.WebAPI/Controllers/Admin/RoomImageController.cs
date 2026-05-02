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

    [HttpGet]
    public async Task<ActionResult<List<RoomImageDto>>> GetRoomImages([FromQuery] Guid roomId)
    {
        var images = await _mediator.Send(new GetRoomImagesQuery { RoomId = roomId });
        return Ok(images);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomImageDto>> GetRoomImage(Guid id)
    {
        var image = await _mediator.Send(new GetRoomImageQuery { Id = id });
        return Ok(image);
    }

    [HttpPost]
    public async Task<ActionResult<RoomImageDto>> CreateRoomImage([FromBody] RoomImageCreateRequest request)
    {
        var image = await _mediator.Send(new CreateRoomImageCommand { Request = request });
        return CreatedAtAction(nameof(GetRoomImage), new { id = image.Id }, image);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoomImageDto>> UpdateRoomImage(Guid id, [FromBody] RoomImageUpdateRequest request)
    {
        var image = await _mediator.Send(new UpdateRoomImageCommand { Id = id, Request = request });
        return Ok(image);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteRoomImage(Guid id)
    {
        var result = await _mediator.Send(new DeleteRoomImageCommand { Id = id });
        return Ok(result);
    }
}