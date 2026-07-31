using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmFollowRecords;

namespace QPS.WebAPI.Controllers.Admin.Crm;

[Route("api/admin/crm/herb-bases/{customerId}/follow-records")]
[ApiController]
public class CrmFollowRecordController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmFollowRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CrmFollowRecordDto>>> GetFollowRecords(Guid customerId)
    {
        var query = new GetCrmFollowRecordsQuery { CustomerId = customerId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CrmFollowRecordDto>> CreateFollowRecord(
        Guid customerId,
        [FromBody] CrmFollowRecordCreateRequest request)
    {
        var command = new CreateCrmFollowRecordCommand { CustomerId = customerId, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}


