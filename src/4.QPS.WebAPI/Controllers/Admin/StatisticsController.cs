using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Statistics;
using QPS.Application.Features.Statistics;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<StatisticsOverviewDto>> GetStatisticsOverview(
        [FromQuery] string? timeRange = "7days")
    {
        var query = new GetStatisticsOverviewQuery { TimeRange = timeRange };
        var overview = await _mediator.Send(query);
        return Ok(overview);
    }

    [HttpGet("realtime")]
    public async Task<ActionResult<StatisticsRealtimeDto>> GetStatisticsRealtime()
    {
        var query = new GetStatisticsRealtimeQuery();
        var realtime = await _mediator.Send(query);
        return Ok(realtime);
    }
}