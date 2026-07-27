using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.System.ChinaRegions;
using QPS.Application.Features.System.ChinaRegions;

namespace QPS.WebAPI.Controllers.Admin.System;

[ApiController]
[Route("api/admin/china-regions")]
[Authorize]
public class ChinaRegionController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChinaRegionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChinaRegionDto>>> GetChinaRegions([FromQuery] bool activeOnly = true)
    {
        var result = await _mediator.Send(new GetChinaRegionsQuery { ActiveOnly = activeOnly });
        return Ok(result);
    }
}
