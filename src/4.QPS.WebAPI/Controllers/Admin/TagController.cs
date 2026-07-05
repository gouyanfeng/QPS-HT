using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Tags;
using QPS.Application.Contracts.Tags;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/tags")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<TagDto>>> GetTags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "TagName",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? tagName = null,
        [FromQuery] string? category = null)
    {
        var query = new GetTagsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            TagName = tagName,
            Category = category
        };
        var tags = await _mediator.Send(query);
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(Guid id)
    {
        var query = new GetTagQuery { Id = id };
        var tag = await _mediator.Send(query);
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] TagCreateRequest request)
    {
        var command = new CreateTagCommand { Request = request };
        var tag = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> UpdateTag(Guid id, [FromBody] TagUpdateRequest request)
    {
        var command = new UpdateTagCommand { Id = id, Request = request };
        var tag = await _mediator.Send(command);
        return Ok(tag);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteTag(Guid id)
    {
        var command = new DeleteTagCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}