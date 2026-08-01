using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmBusinessEntityAttributes;

namespace QPS.WebAPI.Controllers.Admin.Crm;

[Route("api/admin/crm/business-entity-attributes")]
[ApiController]
public class CrmBusinessEntityAttributeController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmBusinessEntityAttributeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CrmBusinessEntityAttributeDto>>> GetAttributes(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromQuery] string? attributeCode = null)
    {
        var query = new GetCrmBusinessEntityAttributesQuery
        {
            EntityType = entityType,
            EntityId = entityId,
            AttributeCode = attributeCode
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<bool>> SaveAttributes([FromBody] CrmBusinessEntityAttributeSaveRequest request)
    {
        var command = new SaveCrmBusinessEntityAttributesCommand { Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
