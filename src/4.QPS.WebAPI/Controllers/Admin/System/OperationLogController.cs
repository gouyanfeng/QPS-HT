using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.System.OperationLogs;
using QPS.Application.Extensions;
using QPS.Application.Features.System.OperationLogs;

namespace QPS.WebAPI.Controllers.Admin.System;

[ApiController]
[Route("api/admin/operation-logs")]
[Authorize]
public class OperationLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationLogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<OperationLogDto>>> GetOperationLogs([FromQuery] OperationLogQueryRequest request)
    {
        var query = new GetOperationLogsQuery { Request = request };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
