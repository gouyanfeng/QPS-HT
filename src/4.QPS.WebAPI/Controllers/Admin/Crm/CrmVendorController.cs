using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Extensions;
using QPS.Application.Features.Crm.CrmVendors;

namespace QPS.WebAPI.Controllers.Admin.Crm;

[Route("api/admin/crm/vendors")]
[ApiController]
public class CrmVendorController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmVendorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<CrmVendorDto>>> GetVendors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "LatestPurchaseTime",
        [FromQuery] string sortDirection = "Descending",
        [FromQuery] string? keyword = null,
        [FromQuery] string? priorityLevel = null,
        [FromQuery] bool? hasPhone = null,
        [FromQuery] bool? hasProduct = null)
    {
        var query = new GetCrmVendorsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Keyword = keyword,
            PriorityLevel = priorityLevel,
            HasPhone = hasPhone,
            HasProduct = hasProduct
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CrmVendorDto>> GetVendor(Guid id)
    {
        var query = new GetCrmVendorQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> CreateVendor([FromBody] CrmVendorCreateRequest request)
    {
        var command = new CreateCrmVendorCommand { Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateVendor(Guid id, [FromBody] CrmVendorUpdateRequest request)
    {
        var command = new UpdateCrmVendorCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("assign-owner")]
    public async Task<ActionResult<bool>> AssignOwner([FromBody] CrmVendorAssignOwnerRequest request)
    {
        var command = new AssignCrmVendorOwnerCommand { Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/contacts")]
    public async Task<ActionResult<bool>> CreateContact(Guid id, [FromBody] CrmContactCreateRequest request)
    {
        var command = new CreateCrmVendorContactCommand { VendorId = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/purchase-plans")]
    public async Task<ActionResult<bool>> CreateVendorPurchasePlan(Guid id, [FromBody] CrmVendorPurchasePlanCreateRequest request)
    {
        var command = new CreateCrmVendorPurchasePlanCommand { VendorId = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}/purchase-plans/{planId}")]
    public async Task<ActionResult<bool>> UpdateVendorPurchasePlan(
        Guid id,
        Guid planId,
        [FromBody] CrmVendorPurchasePlanCreateRequest request)
    {
        var command = new UpdateCrmVendorPurchasePlanCommand { VendorId = id, Id = planId, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}/purchase-plans/{planId}")]
    public async Task<ActionResult<bool>> DeleteVendorPurchasePlan(Guid id, Guid planId)
    {
        var command = new DeleteCrmVendorPurchasePlanCommand { VendorId = id, Id = planId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}/follow-records")]
    public async Task<ActionResult<List<CrmFollowRecordDto>>> GetVendorFollowRecords(Guid id)
    {
        var query = new GetCrmVendorFollowRecordsQuery { VendorId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/follow-records")]
    public async Task<ActionResult<bool>> CreateVendorFollowRecord(Guid id, [FromBody] CrmFollowRecordCreateRequest request)
    {
        var command = new CreateCrmVendorFollowRecordCommand { VendorId = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}/purchase-plans")]
    public async Task<ActionResult<PaginationResponse<CrmVendorPurchasePlanDto>>> GetVendorPurchasePlans(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "PurchaseTime",
        [FromQuery] string sortDirection = "Descending")
    {
        var query = new GetCrmVendorPurchasePlansQuery
        {
            VendorId = id,
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}


