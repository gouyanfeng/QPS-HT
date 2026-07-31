using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmContacts;

namespace QPS.WebAPI.Controllers.Admin.Crm;

[Route("api/admin/crm")]
[ApiController]
public class CrmContactController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmContactController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("customers/{customerId}/contacts")]
    [HttpGet("herb-bases/{customerId}/contacts")]
    public async Task<ActionResult<List<CrmContactDto>>> GetContacts(Guid customerId)
    {
        var query = new GetCrmContactsQuery { CustomerId = customerId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("customers/{customerId}/contacts")]
    [HttpPost("herb-bases/{customerId}/contacts")]
    public async Task<ActionResult<CrmContactDto>> CreateContact(Guid customerId, [FromBody] CrmContactCreateRequest request)
    {
        var command = new CreateCrmContactCommand { CustomerId = customerId, Request = request };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetContacts), new { customerId }, result);
    }

    [HttpPut("contacts/{id}")]
    public async Task<ActionResult<CrmContactDto>> UpdateContact(Guid id, [FromBody] CrmContactUpdateRequest request)
    {
        var command = new UpdateCrmContactCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("contacts/{id}/primary")]
    public async Task<ActionResult<CrmContactDto>> SetPrimary(Guid id)
    {
        var command = new SetPrimaryCrmContactCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("contacts/{id}/status")]
    public async Task<ActionResult<CrmContactDto>> UpdateStatus(Guid id, [FromBody] CrmContactStatusRequest request)
    {
        var command = new UpdateCrmContactStatusCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
