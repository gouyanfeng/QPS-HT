using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;

namespace QPS.WebAPI.Controllers.Admin.Crm;

[Route("api/admin/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("crm")]
    public async Task<ActionResult<CrmDashboardDto>> GetCrmDashboard()
    {
        var result = await _mediator.Send(new GetCrmDashboardQuery());
        return Ok(result);
    }
}
