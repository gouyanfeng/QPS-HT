using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.System.Regions;
using QPS.Application.Extensions;
using QPS.Application.Features.System.Regions;

namespace QPS.WebAPI.Controllers.Admin.System;

[ApiController]
[Route("api/admin/regions")]
[Authorize]
public class RegionController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<RegionDto>>> GetRegions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "SortOrder",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? code = null,
        [FromQuery] string? name = null,
        [FromQuery] int? level = null,
        [FromQuery] Guid? parentId = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetRegionsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Code = code,
            Name = name,
            Level = level,
            ParentId = parentId,
            IsActive = isActive
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RegionDto>> GetRegion(Guid id)
    {
        var result = await _mediator.Send(new GetRegionQuery { Id = id });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RegionDto>> CreateRegion([FromBody] RegionCreateRequest request)
    {
        var result = await _mediator.Send(new CreateRegionCommand { Request = request });
        return CreatedAtAction(nameof(GetRegion), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RegionDto>> UpdateRegion(Guid id, [FromBody] RegionUpdateRequest request)
    {
        var result = await _mediator.Send(new UpdateRegionCommand { Id = id, Request = request });
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRegion(Guid id)
    {
        await _mediator.Send(new DeleteRegionCommand { Id = id });
        return NoContent();
    }
}
