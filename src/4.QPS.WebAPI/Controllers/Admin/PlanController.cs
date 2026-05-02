using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Plans;
using QPS.Application.Contracts.Plans;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/plans")]
[Authorize]
public class PlanController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlanController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<PlanDto>>> GetPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Name",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetPlansQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Name = name,
            IsActive = isActive
        };
        var plans = await _mediator.Send(query);
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlanDto>> GetPlan(Guid id)
    {
        var query = new GetPlanQuery { Id = id };
        var plan = await _mediator.Send(query);
        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<PlanDto>> CreatePlan([FromBody] PlanCreateRequest request)
    {
        var command = new CreatePlanCommand { Request = request };
        var plan = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlanDto>> UpdatePlan(Guid id, [FromBody] PlanUpdateRequest request)
    {
        var command = new UpdatePlanCommand { Id = id, Request = request };
        var plan = await _mediator.Send(command);
        return Ok(plan);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeletePlan(Guid id)
    {
        var command = new DeletePlanCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult<bool>> ToggleStatus(Guid id, [FromBody] bool isActive)
    {
        var command = new TogglePlanStatusCommand { Id = id, IsActive = isActive };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}