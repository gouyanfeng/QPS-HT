using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmFollowRecords;

namespace QPS.WebAPI.Controllers.Admin.Crm;

[Route("api/admin/crm/herb-base-subjects/{herbBaseSubjectId}/follow-records")]
[ApiController]
public class CrmFollowRecordController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmFollowRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CrmFollowRecordDto>>> GetFollowRecords(Guid herbBaseSubjectId)
    {
        var query = new GetCrmFollowRecordsQuery { HerbBaseSubjectId = herbBaseSubjectId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> CreateFollowRecord(
        Guid herbBaseSubjectId,
        [FromBody] CrmFollowRecordCreateRequest request)
    {
        var command = new CreateCrmFollowRecordCommand { HerbBaseSubjectId = herbBaseSubjectId, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}


